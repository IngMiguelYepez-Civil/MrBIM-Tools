# -*- coding: utf-8 -*-
from __future__ import unicode_literals
import os
import sys
import unittest
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', 'MrBIM.extension', 'lib'))
from mrbim.core import parse_names, is_blank, resolve, csv_line

class RulesTests(unittest.TestCase):
    def test_parse_unique_exact_names(self):
        self.assertEqual(parse_names(' Código ; Zona;Código\nClasificación;;'), ['Código', 'Zona', 'Clasificación'])
    def test_numeric_zero_and_boolean_no_are_valid(self):
        self.assertFalse(is_blank('Integer', 0))
        self.assertFalse(is_blank('Double', 0.0))
    def test_missing_blank_and_nonfinite(self):
        self.assertTrue(is_blank('String', ' \t '))
        self.assertTrue(is_blank('Integer', 0, False))
        self.assertTrue(is_blank('Double', float('nan')))
        self.assertTrue(is_blank('Double', float('inf')))
        self.assertTrue(is_blank('String', None))
    def test_elementid_sentinels(self):
        self.assertTrue(is_blank('ElementId', -1))
        self.assertFalse(is_blank('ElementId', -2))
        self.assertFalse(is_blank('ElementId', 5000000000))
    def test_resolution_and_ambiguity(self):
        self.assertEqual(resolve([], [], True)[0], 'NO_EXISTE')
        self.assertEqual(resolve(['a', 'b'], ['c'], True)[0], 'AMBIGUO')
        self.assertEqual(resolve([], ['a', 'b'], True)[0], 'AMBIGUO')
        self.assertEqual(resolve([], ['type'], False)[0], 'NO_EXISTE')
        self.assertEqual(resolve([], ['type'], True), ('ENCONTRADO', 'type', 'Tipo'))
    def test_instance_empty_still_has_priority(self):
        self.assertEqual(resolve([''], ['type value'], True), ('ENCONTRADO', '', 'Instancia'))
    def test_csv_roundtrip_and_formula_guard(self):
        import csv
        import io
        data = ['Código; zona', 'Texto "doble"\ny línea', '=1+1', '  @SUM(A1)', 0]
        row = next(csv.reader(io.StringIO(csv_line(data)), delimiter=';'))
        self.assertEqual(row, ['Código; zona', 'Texto "doble"\ny línea', "'=1+1", "'  @SUM(A1)", '0'])

if __name__ == '__main__':
    unittest.main()
