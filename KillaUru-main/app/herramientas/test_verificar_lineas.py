"""008 T005 / FR-076: fronteras del control constitucional, patrón AAA."""

import tempfile
import unittest
from pathlib import Path

from verificar_lineas import infracciones


class VerificarLineasTest(unittest.TestCase):
    def test_199_lineas_se_aceptan_y_200_se_rechazan(self):
        # Arrange
        with tempfile.TemporaryDirectory() as carpeta:
            raiz = Path(carpeta)
            (raiz / "Valido.kt").write_text("// línea\n" * 199, encoding="utf-8")
            (raiz / "Largo.kt").write_text("// línea\n" * 200, encoding="utf-8")
            # Act
            resultado = infracciones(raiz)
            # Assert
            self.assertEqual(resultado, [("Largo.kt", 200)])

    def test_excluye_build_schemas_y_wrapper_generado(self):
        # Arrange
        with tempfile.TemporaryDirectory() as carpeta:
            raiz = Path(carpeta)
            for archivo in ("build/Generado.kt", "schemas/Esquema.kt", "gradlew"):
                destino = raiz / archivo
                destino.parent.mkdir(parents=True, exist_ok=True)
                destino.write_text("// generado\n" * 300, encoding="utf-8")
            # Act
            resultado = infracciones(raiz)
            # Assert
            self.assertEqual(resultado, [])

    def test_cubre_scripts_y_configuracion_kotlin(self):
        # Arrange
        with tempfile.TemporaryDirectory() as carpeta:
            raiz = Path(carpeta)
            for nombre in ("build.gradle.kts", "control.py", "publicar.sh"):
                (raiz / nombre).write_text("# línea\n" * 200, encoding="utf-8")
            # Act
            resultado = infracciones(raiz)
            # Assert
            self.assertEqual(len(resultado), 3)


if __name__ == "__main__":
    unittest.main()
