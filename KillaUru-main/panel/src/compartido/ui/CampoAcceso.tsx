// Campo de las pantallas de acceso (login, recuperar, fijar contraseña): etiqueta visible unida a su input por `id`
interface Props {
  nombre: string
  etiqueta: string
  tipo: 'email' | 'password'
  autocompletar: 'email' | 'current-password' | 'new-password'
  /** Sin valor ni onCambiar el campo no es controlado y se lee del formulario al enviar (fijar contraseña). */
  valor?: string
  onCambiar?: (valor: string) => void
}

export function CampoAcceso({ nombre, etiqueta, tipo, autocompletar, valor, onCambiar }: Props) {
  const controlado = onCambiar ? { value: valor, onChange: (evento: React.ChangeEvent<HTMLInputElement>) => onCambiar(evento.target.value) } : {}
  return (
    <div className="field">
      <label htmlFor={nombre}>{etiqueta}</label>
      <input id={nombre} name={nombre} type={tipo} autoComplete={autocompletar} required={Boolean(onCambiar)} {...controlado} />
    </div>
  )
}
