# -*- coding: utf-8 -*-
from __future__ import unicode_literals
"""MrBIM Tools 0.1.0 - Revisor de parametros obligatorios."""
import codecs
import datetime
from pyrevit import DB, revit, forms, script
from System.Collections.Generic import List
from mrbim.core import parse_names, resolve, is_blank, csv_line
try:
    text_type = unicode
except NameError:
    text_type = str


def eid_value(eid):
    # Revit 2024/2025 usan ElementId de 64 bits.
    return int(eid.Value)


def safe_html(value):
    return text_type(value).replace('&', '&amp;').replace('<', '&lt;').replace('>', '&gt;').replace('"', '&quot;')


def label(element):
    try:
        return element.Name
    except Exception:
        return text_type(element.Id.Value)


def parameters(element, name):
    if element is None:
        return []
    return list(element.GetParameters(name))


def check_parameter(element, name, allow_type, doc):
    try:
        instance = parameters(element, name)
        type_params = []
        if not instance and allow_type:
            type_element = doc.GetElement(element.GetTypeId())
            type_params = parameters(type_element, name)
        state, param, source = resolve(instance, type_params, allow_type)
        if state != 'ENCONTRADO':
            return state, source, ''
        kind = text_type(param.StorageType)
        if kind == 'String':
            value = param.AsString()
        elif kind == 'Integer':
            value = param.AsInteger()
        elif kind == 'Double':
            value = param.AsDouble()
        elif kind == 'ElementId':
            value = eid_value(param.AsElementId())
        else:
            return 'NO_EVALUABLE', source, 'StorageType: ' + kind
        return ('VACIO' if is_blank(kind, value, param.HasValue) else 'OK'), source, ''
    except Exception as error:
        return 'ERROR_LECTURA', '', text_type(error)


def main():
    doc = revit.doc
    if doc is None or doc.IsFamilyDocument:
        forms.alert('Abra un proyecto de Revit para ejecutar la revision.')
        return
    year = text_type(revit.app.VersionNumber)
    if year not in ('2024', '2025'):
        forms.alert('Este prototipo esta dirigido a Revit 2024 y 2025. Version detectada: ' + year)
        return
    scope = forms.CommandSwitchWindow.show(
        ['Seleccion actual', 'Vista activa', 'Proyecto completo'],
        message='MrBIM | Alcance de la revision (modelo anfitrion)')
    if not scope:
        return
    if scope == 'Seleccion actual':
        elements = [doc.GetElement(i) for i in revit.uidoc.Selection.GetElementIds()]
    else:
        try:
            collector = (DB.FilteredElementCollector(doc, doc.ActiveView.Id)
                         if scope == 'Vista activa' else DB.FilteredElementCollector(doc))
            elements = list(collector.WhereElementIsNotElementType())
        except Exception as error:
            forms.alert('No se pudo consultar esta vista. Use otra vista o Proyecto completo.\n' + text_type(error))
            return
    elements = [e for e in elements if e is not None and not isinstance(e, DB.ElementType)
                and e.Category is not None and e.Category.CategoryType == DB.CategoryType.Model]
    if not elements:
        forms.alert('No hay elementos de categorias de modelo en este alcance.')
        return
    categories = {}
    for element in elements:
        cid = eid_value(element.Category.Id)
        categories[cid] = element.Category.Name
    category_labels = dict((u'{} [{}]'.format(name, cid), cid) for cid, name in categories.items())
    picked = forms.SelectFromList.show(sorted(category_labels), multiselect=True,
                                      title='MrBIM | Categorias a revisar', button_name='Continuar')
    if not picked:
        return
    selected = set(category_labels[x] for x in picked)
    elements = [e for e in elements if eid_value(e.Category.Id) in selected]
    raw = forms.ask_for_string(
        default='', title='MrBIM | Parametros obligatorios',
        prompt='Escriba nombres EXACTOS, separados por punto y coma.\n'
               'Ejemplo: Codigo; Clasificacion; Zona\n'
               'Use los nombres que aparecen en su modelo, incluidos tildes y mayusculas.')
    names = parse_names(raw or '')
    if not names:
        return
    policy = forms.CommandSwitchWindow.show(
        ['Solo instancia', 'Instancia y, si no existe, tipo'],
        message='MrBIM | Donde buscar cada parametro')
    if not policy:
        return
    allow_type = policy != 'Solo instancia'
    findings, failed_ids = [], {}
    checked, total_checks, ok = 0, 0, 0
    counts = {}
    with forms.ProgressBar(title='MrBIM | Revisando {value} de {max_value}', cancellable=True, step=20) as progress:
        for index, element in enumerate(elements):
            if progress.cancelled:
                forms.alert('Revision cancelada. No se genera un informe parcial.')
                return
            for name in names:
                state, source, detail = check_parameter(element, name, allow_type, doc)
                total_checks += 1
                if state == 'OK':
                    ok += 1
                    continue
                counts[state] = counts.get(state, 0) + 1
                item = dict(element_id=eid_value(element.Id), unique_id=element.UniqueId,
                            category=element.Category.Name, element=label(element),
                            parameter=name, state=state, source=source, detail=detail)
                findings.append(item)
                failed_ids[item['element_id']] = element.Id
            checked += 1
            progress.update_progress(index + 1, len(elements))
    output = script.get_output()
    output.set_title('MrBIM Tools | Revision de parametros')
    output.print_md('## MrBIM Tools — Revision de parametros')
    output.print_html('<p>Proyecto: <b>{}</b> | Revit {} | Alcance: {}<br>'
                      'Busqueda: {} | Modelo anfitrion; no incluye contenido de vinculos.</p>'.format(
                          safe_html(doc.Title), safe_html(year), safe_html(scope), safe_html(policy)))
    output.print_md('Elementos revisados: **{}** | Comprobaciones: **{}** | OK: **{}** | Hallazgos: **{}**'.format(
        checked, total_checks, ok, len(findings)))
    output.print_html('<p>Parametros: {}</p>'.format(safe_html('; '.join(names))))
    for state in sorted(counts):
        output.print_md('{}: **{}**'.format(state, counts[state]))
    if findings:
        # Limite de pantalla; el CSV siempre contiene TODOS los hallazgos.
        rows = []
        for item in findings[:200]:
            link = output.linkify(failed_ids[item['element_id']])
            rows.append('<tr><td>{}</td><td>{}</td><td>{}</td><td>{}</td><td>{}</td></tr>'.format(
                link, safe_html(item['category']), safe_html(item['element']),
                safe_html(item['parameter']), safe_html(item['state'])))
        output.print_html('<table><tr><th>ID</th><th>Categoria</th><th>Elemento</th><th>Parametro</th><th>Estado</th></tr>'
                          + ''.join(rows) + '</table>')
        if len(findings) > 200:
            output.print_md('Se muestran los primeros 200 hallazgos. Exporte el CSV para ver todos.')
    else:
        output.print_md('Sin hallazgos para los parametros, categorias y alcance seleccionados.')
    if forms.alert('¿Exportar el informe completo a CSV?', yes=True, no=True):
        path = forms.save_file(file_ext='csv', default_name='MrBIM_Parametros_' + datetime.datetime.now().strftime('%Y%m%d_%H%M%S'))
        if path:
            try:
                keys = ['element_id', 'unique_id', 'category', 'element', 'parameter', 'state', 'source', 'detail']
                with codecs.open(path, 'w', 'utf-8-sig') as stream:
                    stream.write(csv_line(['Proyecto', 'Revit', 'Fecha', 'Alcance', 'Busqueda',
                                           'Elementos_revisados', 'Comprobaciones', 'OK', 'Hallazgos',
                                           'ElementId', 'UniqueId', 'Categoria', 'Elemento', 'Parametro', 'Estado', 'Origen', 'Detalle']))
                    stamp = datetime.datetime.now().isoformat()
                    meta = [doc.Title, year, stamp, scope, policy, checked, total_checks, ok, len(findings)]
                    if findings:
                        for item in findings:
                            stream.write(csv_line(meta + [item[k] for k in keys]))
                    else:
                        stream.write(csv_line(meta + ['', '', '', '', '; '.join(names), 'SIN_HALLAZGOS', '', '']))
                forms.alert('Informe guardado:\n' + path)
            except Exception as error:
                forms.alert('No se pudo guardar el CSV. El informe sigue en pantalla.\n' + text_type(error))
    if failed_ids and forms.alert('¿Seleccionar los elementos con hallazgos en Revit?', yes=True, no=True):
        revit.uidoc.Selection.SetElementIds(List[DB.ElementId](list(failed_ids.values())))


if __name__ == '__main__':
    main()
