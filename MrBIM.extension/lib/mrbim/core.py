# -*- coding: utf-8 -*-
from __future__ import unicode_literals
"""Reglas independientes de Revit. Compatible con Python 2.7 y 3."""
import math
try:
    text_type = unicode
except NameError:
    text_type = str


def parse_names(value):
    """Nombres exactos, separados por punto y coma; elimina duplicados."""
    result = []
    for item in value.replace('\r', '\n').replace('\n', ';').split(';'):
        name = item.strip()
        if name and name not in result:
            result.append(name)
    return result


def is_blank(storage, value, has_value=True):
    if not has_value or value is None:
        return True
    if storage == 'String':
        return not text_type(value).strip()
    if storage == 'Double':
        return math.isnan(value) or math.isinf(value)
    if storage == 'ElementId':
        # Otros IDs negativos pueden representar opciones validas de Revit.
        return value == -1
    return False  # Cero numerico y No (0) son valores validos.


def resolve(instance, type_parameters, allow_type):
    """No adivina cuando varios parametros comparten nombre."""
    if len(instance) > 1:
        return 'AMBIGUO', None, 'Instancia'
    if instance:
        return 'ENCONTRADO', instance[0], 'Instancia'
    if allow_type:
        if len(type_parameters) > 1:
            return 'AMBIGUO', None, 'Tipo'
        if type_parameters:
            return 'ENCONTRADO', type_parameters[0], 'Tipo'
    return 'NO_EXISTE', None, ''


def csv_cell(value):
    s = '' if value is None else text_type(value)
    # Evita que contenido del modelo se interprete como formula al abrir Excel.
    if s.lstrip().startswith(('=', '+', '-', '@', '\t', '\r', '\n')) or s.startswith(('\t', '\r', '\n')):
        s = "'" + s
    return '"' + s.replace('"', '""') + '"'


def csv_line(values):
    return ';'.join(csv_cell(x) for x in values) + '\r\n'
