// T074 — textos de la pantalla Resumen (FR-015…FR-020)
export const TEXTOS_RESUMEN = {
  cargando: {
    indicadores: 'Cargando indicadores…',
    franja: 'Cargando franja…',
    ultimosCambios: 'Cargando cambios…',
    atencion: 'Cargando elementos de atención…',
  },
  vacio: {
    indicadores: 'Aún no hay indicadores calculados.',
    franja: 'No hay anomalías activas en el corredor.',
    ultimosCambios: 'Sin cambios recientes.',
    atencion: 'Nada requiere atención ahora.',
  },
  error: {
    indicadores: 'No se pudo cargar los indicadores.',
    franja: 'No se pudo cargar la franja.',
    ultimosCambios: 'No se pudo cargar los cambios.',
    atencion: 'No se pudo cargar los elementos de atención.',
  },
  atencion: {
    porRevisar: 'Puntos descartados con reportes nuevos',
    porRevisarDetalle: '3 o más vehículos en 7 días',
    pendientes: 'Esperan confirmación',
    revision: 'Revisión mensual de privilegios',
    revisionVencida: 'Vencida',
    estado: 'Estado del servicio',
    estadoDetalle: 'Página pública de UptimeRobot',
    temas: (n: number) => `${n} ${n === 1 ? 'tema' : 'temas'}`,
  },
  indicadores: {
    variacionSufijo: 'vs. 7 d previos',
    sinCalcular: 'Sin calcular',
  },
  grafico: {
    titulo: 'Eventos válidos por día',
    periodoActual: 'Período actual',
    periodoAnterior: 'Período anterior',
  },
  panelesTecnicos: 'Paneles técnicos',
  ultimosCambios: {
    titulo: 'Últimos cambios de estado',
    sistema: 'sistema',
  },
  franjaCorredor: 'Anomalías activas a lo largo del corredor',
  selector: {
    dias7: '7 d',
    dias14: '14 d',
    dias30: '30 d',
  },
}
