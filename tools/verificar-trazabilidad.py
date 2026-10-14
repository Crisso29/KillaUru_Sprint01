"""Verifica la trazabilidad de docs/01 (análisis), docs/02 (arquitectura), docs/03 (diseño) y docs/04 (C4).

Uso (desde la raíz del repositorio):
    python tools/verificar-trazabilidad.py

Comprueba que los IDs y las tablas coincidan entre sí. No juzga si una relación
tiene sentido: eso sigue siendo una revisión humana.
Termina con código 1 si encuentra errores, para poder usarlo en CI.
"""
import collections
import pathlib
import re
import sys

sys.stdout.reconfigure(encoding='utf-8')   # la consola de Windows no siempre es UTF-8

RAIZ = pathlib.Path(__file__).resolve().parent.parent / 'docs'
A = RAIZ / '01-analisis-del-sistema'
B = RAIZ / '02-arquitectura-de-software'
C = RAIZ / '03-diseño-de-software'
D = RAIZ / '04-modelo-C4'
DOCS = {p.relative_to(RAIZ).as_posix(): p.read_text(encoding='utf-8')
        for p in sorted(list(A.glob('*.md')) + list(B.glob('*.md')) + list(C.glob('*.md')) + list(D.glob('*.md')))}
ID = r'(?:HU-\d+b?|RF-\d+|RNF-\d+|RN-\d+|EQ-\d+|DA-\d+|ADR-\d+|AC-\d+|RS-\d+|R-\d+|S-\d+|EP-\d+)'
errores = []


def doc(nombre):
    return DOCS['01-analisis-del-sistema/' + nombre]


def error(grupo, mensaje):
    errores.append((grupo, mensaje))


def expandir(texto):
    """IDs citados en el texto, expandiendo rangos como RNF-08…11 o HU-01…HU-04."""
    ids = set(re.findall(r'\b' + ID + r'\b', texto))
    for m in re.finditer(r'\b(HU|RF|RNF|RN|EQ|DA|ADR|AC|RS|R|S)-(\d+)\s*(?:…|\.\.\.)\s*(?:\1-)?(\d+)', texto):
        pref, ini, fin = m.group(1), int(m.group(2)), int(m.group(3))
        for k in range(ini, fin + 1):
            ids.add(f'{pref}-{k:0{len(m.group(2))}d}')
    return ids


def orden(i):
    return (int(re.search(r'\d+', i).group()), i)


# ---------------------------------------------------------------- definiciones
DEF = {
    'HU': ('02-historias-de-usuario.md', r'^\| (HU-\d+b?) \| Como'),
    'RF': ('03-requisitos-funcionales.md', r'^\| (RF-\d+) \| El sistema'),
    'RNF': ('04-atributos-de-calidad.md', r'^\| (RNF-\d+) \|'),
    'AC': ('04-atributos-de-calidad.md', r'^\| (AC-\d+) \|'),
    'R': ('05-restricciones.md', r'^\| (R-\d+) \|'),
    'S': ('05-restricciones.md', r'^\| (S-\d+) \|'),
    'RS': ('05-restricciones.md', r'^\| (RS-\d+) \|'),
    'DA': ('06-driver-arquitectonicos.md', r'^\| (DA-\d+) \|'),
    'ADR': ('07-decisiones-arquitectonicas.md', r'^## (ADR-\d+) '),
    'EQ': ('08-escenarios-y-atributos-de-calidad.md', r'^## (EQ-\d+)\.'),
    'RN': ('09-reglas-de-negocio.md', r'^### (RN-\d+) |^\| (RN-\d+) \|'),
    'EP': ('02-historias-de-usuario.md', r'^## (EP-\d+):'),
}
definidos = {}
for tipo, (archivo, patron) in DEF.items():
    definidos[tipo] = {[g for g in m.groups() if g][0] for m in re.finditer(patron, doc(archivo), re.M)}
todos = set().union(*definidos.values())

# 1. IDs citados que no existen
for nombre, texto in DOCS.items():
    for i in sorted(expandir(texto) - todos, key=orden):
        error('IDs inexistentes', f'{nombre} cita {i}, que no está definido')

# 2. IDs definidos que ningún otro documento usa
for tipo in ('HU', 'RF', 'RNF', 'RN', 'DA', 'ADR', 'EQ'):
    for i in definidos[tipo]:
        if not any(i in expandir(t) for n, t in DOCS.items() if not n.endswith(DEF[tipo][0])):
            error('IDs sin uso', f'{i} solo aparece en {DEF[tipo][0]}')

# 3. HU → RF: filas de RF (fuente de verdad) contra la tabla inversa
rf_doc = doc('03-requisitos-funcionales.md')
principal, apoyo = collections.defaultdict(set), collections.defaultdict(set)
for m in re.finditer(r'^\| (RF-\d+) \| .*? \| (HU-\d+b?) \| ([^|]*) \| [^|]* \|$', rf_doc, re.M):
    principal[m.group(2)].add(m.group(1))
    for h in re.findall(r'HU-\d+b?', m.group(3)):
        apoyo[h].add(m.group(1))
ini = rf_doc.index('## Trazabilidad inversa')
fin = rf_doc.index('\n## ', ini + 5)
inversa = {m.group(1): (set(re.findall(r'RF-\d+', m.group(2))), set(re.findall(r'RF-\d+', m.group(3))))
           for m in re.finditer(r'^\| (HU-\d+b?) [^|]*\| ([^|]*) \| ([^|]*) \|$', rf_doc[ini:fin], re.M)}
for hu in sorted(definidos['HU'], key=orden):
    if hu not in inversa:
        error('HU → RF', f'{hu} no está en la tabla inversa')
    elif inversa[hu] != (principal[hu], apoyo[hu]):
        error('HU → RF', f'{hu}: filas de RF dicen principal {sorted(principal[hu])} apoyo {sorted(apoyo[hu])}; '
                         f'la tabla inversa dice {sorted(inversa[hu][0])} / {sorted(inversa[hu][1])}')

# 4. Cada HU en MoSCoW, en el mapa de épicas, con la misma prioridad y épica, y con actor
hu_doc = doc('02-historias-de-usuario.md')
def seccion(texto, desde, hasta=None):
    i = texto.index(desde)
    return texto[i: texto.index(hasta, i) if hasta else len(texto)]
moscow = set(re.findall(r'HU-\d+b?', seccion(hu_doc, '## Resumen por prioridad', '**Secuencia')))
epicas = set(re.findall(r'HU-\d+b?', seccion(hu_doc, '## Mapa de épicas')))
for hu in sorted(definidos['HU'] - moscow, key=orden):
    error('Historias', f'{hu} no está en el resumen MoSCoW')
for hu in sorted(definidos['HU'] - epicas, key=orden):
    error('Historias', f'{hu} no está en el mapa de épicas')
prio_tabla = {m.group(1): m.group(2) for m in re.finditer(r'^\| (HU-\d+b?) \| .*? \| ([MSC]) \|', hu_doc, re.M)}
prio_resumen = {}
for etiqueta, letra in (('Must', 'M'), ('Should', 'S'), ('Could', 'C')):
    fila = re.search(r'^\| \*\*' + etiqueta + r'\*\*[^|]*\| ([^|]*) \|', hu_doc, re.M).group(1)
    prio_resumen.update({h: letra for h in re.findall(r'HU-\d+b?', fila)})
for hu, letra in prio_tabla.items():
    if prio_resumen.get(hu) != letra:
        error('Historias', f'{hu}: prioridad {letra} en su tabla y {prio_resumen.get(hu)} en el resumen')
epica_seccion, epica_mapa = {}, {}
for m in re.finditer(r'^## (EP-\d+):', hu_doc, re.M):
    sig = hu_doc.find('\n## ', m.end())
    for h in re.findall(r'^\| (HU-\d+b?) \|', hu_doc[m.end(): sig if sig > 0 else None], re.M):
        epica_seccion[h] = m.group(1)
for m in re.finditer(r'^\| (EP-\d+) [^|]*\| ([^|]*) \|', hu_doc, re.M):
    epica_mapa.update({h: m.group(1) for h in re.findall(r'HU-\d+b?', m.group(2))})
for hu, ep in epica_seccion.items():
    if epica_mapa.get(hu) != ep:
        error('Historias', f'{hu}: está en la sección {ep} pero el mapa dice {epica_mapa.get(hu)}')
actores = expandir(seccion(doc('01-actores.md'), '## 6. Trazabilidad'))
for hu in sorted(definidos['HU'] - actores, key=orden):
    error('Historias', f'{hu} no tiene actor en 01-actores.md §6')

# 5. RNF y escenarios dentro de un atributo de calidad, y AC ↔ atributo del escenario
q_doc = doc('04-atributos-de-calidad.md')
resumen_ac = seccion(q_doc, '## Resumen de prioridades')
for i in sorted(definidos['RNF'] - expandir(resumen_ac), key=orden):
    error('Atributos de calidad', f'{i} no pertenece a ningún AC')
ac_eq = {m.group(1): set(re.findall(r'EQ-\d+', m.group(2))) for m in re.finditer(r'^\| (AC-\d+) \|.*\| ([^|]*) \|$', q_doc, re.M)}
eq_doc = doc('08-escenarios-y-atributos-de-calidad.md')
eq_traza, eq_atrib = {}, {}
for m in re.finditer(r'^## (EQ-\d+)\.(.*?)(?=^## )', eq_doc, re.M | re.S):
    cuerpo = m.group(2)
    eq_traza[m.group(1)] = set(re.findall(r'DA-\d+', re.search(r'\*\*Trazabilidad\*\* \| ([^\n]*)', cuerpo).group(1)))
    eq_atrib[m.group(1)] = set(re.findall(r'AC-\d+', re.search(r'\*\*Atributo\*\* \| ([^\n]*)', cuerpo).group(1)))
    if not re.search(r'^\| ' + m.group(1) + r' \|', eq_doc, re.M):
        error('Escenarios', f'{m.group(1)} no está en el resumen de escenarios')
for eq, acs in eq_atrib.items():
    for ac in acs:
        if eq not in ac_eq.get(ac, set()):
            error('AC ↔ EQ', f'{eq} dice atributo {ac}, pero {ac} no lista {eq}')
for ac, eqs in ac_eq.items():
    for eq in eqs:
        if ac not in eq_atrib.get(eq, set()):
            error('AC ↔ EQ', f'{ac} lista {eq}, pero {eq} dice atributo {sorted(eq_atrib.get(eq, set()))}')

# 6. Drivers ↔ ADR y drivers ↔ escenarios (en ambos sentidos)
da_doc = doc('06-driver-arquitectonicos.md')
adr_doc = doc('07-decisiones-arquitectonicas.md')
da_fila = {m.group(1): m.group(0).split('|') for m in re.finditer(r'^\| (DA-\d+) \|.*$', da_doc, re.M)}
da_adr = {d: set(re.findall(r'ADR-\d+', c[8])) for d, c in da_fila.items()}
da_eq = {d: set(re.findall(r'EQ-\d+', c[6])) for d, c in da_fila.items()}
adr_da = {m.group(1): set(re.findall(r'DA-\d+', m.group(2)))
          for m in re.finditer(r'^## (ADR-\d+) [^\n]*\n\n\*\*Estado:\*\*([^\n]*)', adr_doc, re.M)}
adr_resumen = {m.group(1): set(re.findall(r'DA-\d+', m.group(2)))
               for m in re.finditer(r'^\| \*\*(ADR-\d+)\*\* \|[^|]*\|[^|]*\|([^|]*)\|', adr_doc, re.M)}
for adr, das in adr_da.items():
    if not das:
        error('DA ↔ ADR', f'{adr} no tiene drivers')
    if das != adr_resumen.get(adr):
        error('DA ↔ ADR', f'{adr}: encabezado {sorted(das)} y tabla resumen {sorted(adr_resumen.get(adr, set()))}')
for da, adrs in da_adr.items():
    for adr in adrs:
        if da not in adr_da.get(adr, set()):
            error('DA ↔ ADR', f'{da} cita {adr}, pero {adr} no lista {da}')
for adr, das in adr_da.items():
    for da in das:
        if adr not in da_adr.get(da, set()):
            error('DA ↔ ADR', f'{adr} lista {da}, pero {da} no cita {adr}')
for da, eqs in da_eq.items():
    for eq in eqs:
        if da not in eq_traza.get(eq, set()):
            error('DA ↔ EQ', f'{da} cita {eq}, pero la trazabilidad de {eq} no lista {da}')
for eq, das in eq_traza.items():
    for da in das:
        if eq not in da_eq.get(da, set()):
            error('DA ↔ EQ', f'{eq} lista {da}, pero {da} no cita {eq}')

# 7. Cada RF dentro de algún componente (backend, app o panel)
comp = DOCS['02-arquitectura-de-software/componentes-arquitectonicos.md']
en_componentes = set(re.findall(r'RF-\d+', seccion(comp, '## 5. Componentes de la app', '## 6.'))) \
    | set(re.findall(r'RF-\d+', seccion(comp, '## 9. Trazabilidad', '## 10.')))
for rf in sorted(definidos['RF'] - en_componentes, key=orden):
    error('Componentes', f'{rf} no aparece en ningún componente')

# 8. Términos de diseños anteriores que ya no deben aparecer (señal de un documento desactualizado)
OBSOLETOS = ['DispositivoHash', 'Dapper', 'SendGrid', 'ASP.NET Core 8', '.NET 8)', 'IColaEventos', 'ColaEventosChannel',
             'IValidacionService', 'IClasificacionService', 'ValidarAnomaliaUseCase', 'ClasificarPersistenciaUseCase',
             'status.killauru.pe', 'UltimoReporteUtc', 'RadioMetros', 'ViaLibre', 'Leaflet', 'JWT 8 h']
PERMITIDAS = ('Se descarta .NET 8', 'SendGrid **retiró', 'el plan gratuito de SendGrid', 'ya no existe `status.killauru.pe`')
for nombre, texto in DOCS.items():
    if nombre.endswith('07-decisiones-arquitectonicas.md'):   # los ADR registran alternativas descartadas
        continue
    for termino in OBSOLETOS:
        if any(termino in l and not any(p in l for p in PERMITIDAS) for l in texto.splitlines()):
            error('Términos obsoletos', f'{nombre} usa «{termino}»')

# 9. Cada parámetro de una regla de negocio tiene dónde configurarse (02 · enfoque arquitectónico)
rn_doc = doc('09-reglas-de-negocio.md')
enfoque = DOCS['02-arquitectura-de-software/enfoque-arquitectonico.md']
parametros = set(re.findall(r'`([A-Z][a-z]+(?:[A-Z][a-z]+)+)`', seccion(rn_doc, '## 2. Reglas en el dispositivo', '## 7. Trazabilidad')))
parametros = {p for p in parametros if re.search(r'(Horas|Dias|Minutos|Segundos|Metros|Kmh|Minimo|Maxima|Maximo|Requeridos'
                                                 r'|Desde|Porcentaje|Detectan|Minimos|Maximos|Descartada|Revision)$', p)}
for p in sorted(parametros):
    if p not in enfoque:
        error('Parámetros', f'{p} (09) no tiene dónde configurarse en enfoque-arquitectonico.md')

# ---------------------------------------------------------------- reporte
print(f'Documentos revisados: {len(DOCS)}')
print('Definidos: ' + ', '.join(f'{k} {len(v)}' for k, v in definidos.items()))
if not errores:
    print('\nOK: 0 errores de trazabilidad.')
    sys.exit(0)
print(f'\n{len(errores)} errores:')
for grupo, msg in errores:
    print(f'  [{grupo}] {msg}')
sys.exit(1)
