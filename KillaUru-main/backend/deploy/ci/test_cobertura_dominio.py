"""Prueba que el CI falla sin informes o con un dominio insuficiente."""
import contextlib
import io
import tempfile
import unittest
from pathlib import Path
from cobertura_dominio import verificar


class CoberturaTests(unittest.TestCase):
    """Informes sintéticos aislados de las coberturas reales del repositorio."""

    def evaluar(self, cobertura):
        with tempfile.TemporaryDirectory() as temporal:
            raiz = Path(temporal)
            for nombre in ('Uno', 'Dos'):
                carpeta = raiz / 'src' / nombre / 'Domain'
                carpeta.mkdir(parents=True)
                (carpeta / 'Regla.cs').write_text(f'namespace {nombre}.Domain;')
            if cobertura is not None:
                clases = ''.join(f'<class name="{n}.Domain.Regla"><lines><line number="1" hits="{h}"/></lines></class>'
                                 for n, h in cobertura.items())
                (raiz / 'prueba.cobertura.xml').write_text(f'<coverage>{clases}</coverage>')
            with contextlib.redirect_stdout(io.StringIO()):
                return verificar(raiz, raiz, 90)

    def test_sin_informes_falla(self):
        self.assertEqual(1, self.evaluar(None))

    def test_dominio_ausente_falla(self):
        self.assertEqual(1, self.evaluar({'Uno': 1}))

    def test_un_dominio_no_compensa_otro(self):
        self.assertEqual(1, self.evaluar({'Uno': 1, 'Dos': 0}))

    def test_ambos_dominios_cubiertos(self):
        self.assertEqual(0, self.evaluar({'Uno': 1, 'Dos': 1}))


if __name__ == '__main__':
    unittest.main()
