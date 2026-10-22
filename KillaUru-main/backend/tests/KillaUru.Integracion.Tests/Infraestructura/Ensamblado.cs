using KillaUru.Integracion.Tests.Infraestructura;

// Un solo PostgreSQL para toda la ejecución: arrancarlo cuesta segundos y las pruebas no se pisan
// porque cada una trabaja con sus propios datos o solo lee la estructura.
[assembly: AssemblyFixture(typeof(BaseDatosPrueba))]
