// Panel de la veterinaria. Consume la API del mismo sitio con fetch, por eso no depende de CORS.
// Solo trabaja con los DTOs que devuelve la API. Los datos se insertan siempre como texto
// (nunca con innerHTML) para evitar inyección de código.

const $ = (selector, raiz = document) => raiz.querySelector(selector);
const SVG = 'http://www.w3.org/2000/svg';

class ErrorApi extends Error {
  constructor(codigo, cuerpo, mensaje) {
    super(mensaje);
    this.codigo = codigo;
    this.cuerpo = cuerpo;
  }
}

async function pedir(ruta, { metodo = 'GET', datos } = {}) {
  let respuesta;
  try {
    respuesta = await fetch(`/api${ruta}`, {
      method: metodo,
      headers: datos ? { 'Content-Type': 'application/json' } : undefined,
      body: datos ? JSON.stringify(datos) : undefined,
    });
  } catch {
    throw new ErrorApi(0, null, 'No se pudo conectar con la API. Revisa que esté en ejecución e inténtalo de nuevo.');
  }

  if (respuesta.status === 204) return null;

  const cuerpo = await respuesta.json().catch(() => null);
  if (!respuesta.ok) {
    // Los errores llegan como ProblemDetails: el 409 trae la regla incumplida en "detail"
    throw new ErrorApi(respuesta.status, cuerpo, cuerpo?.detail ?? cuerpo?.title ?? 'Ocurrió un error inesperado.');
  }
  return cuerpo;
}

// --- Utilidades de interfaz ---

function el(etiqueta, atributos = {}, ...hijos) {
  const nodo = document.createElement(etiqueta);
  for (const [nombre, valor] of Object.entries(atributos)) {
    if (nombre.startsWith('on')) nodo[nombre] = valor;
    else nodo.setAttribute(nombre, valor);
  }
  nodo.append(...hijos);
  return nodo;
}

function icono(nombre) {
  const svg = document.createElementNS(SVG, 'svg');
  svg.setAttribute('class', 'icono');
  svg.setAttribute('aria-hidden', 'true');
  const uso = document.createElementNS(SVG, 'use');
  uso.setAttribute('href', `#i-${nombre}`);
  svg.append(uso);
  return svg;
}

function avisar(mensaje, tipo = 'exito') {
  const toast = el('div', { class: `toast toast--${tipo}` }, icono(tipo === 'exito' ? 'listo' : 'alerta'), el('span', {}, mensaje));
  const avisos = $('#avisos');
  avisos.append(toast);
  // Si se acumulan, se quita el más antiguo para no tapar la tabla
  if (avisos.children.length > 3) avisos.firstElementChild.remove();
  setTimeout(() => toast.remove(), 4000);
}

function mostrarEstadoApi(estado, texto) {
  const indicador = $('#estado-api');
  indicador.dataset.estado = estado;
  indicador.textContent = texto;
}

function mostrarCargando(cuerpo, columnas) {
  cuerpo.replaceChildren(...Array.from({ length: 4 }, () =>
    el('tr', { class: 'fila-esqueleto' }, ...Array.from({ length: columnas }, () =>
      el('td', {}, el('span', { class: 'esqueleto' }))))));
}

function mostrarMensaje(cuerpo, columnas, titulo, detalle, accion) {
  const contenido = el('div', { class: 'estado-vacio' },
    el('p', { class: 'estado-vacio__titulo' }, titulo),
    el('p', { class: 'estado-vacio__detalle' }, detalle));
  if (accion) {
    contenido.append(el('button', { class: 'boton boton--secundario', type: 'button', onclick: accion.alPulsar }, accion.texto));
  }
  cuerpo.replaceChildren(el('tr', { class: 'fila-estado' }, el('td', { colspan: columnas }, contenido)));
}

function botonAccion(nombreIcono, etiqueta, alPulsar, clase = '') {
  return el('button', { class: `boton-icono ${clase}`, type: 'button', title: etiqueta, 'aria-label': etiqueta, onclick: alPulsar }, icono(nombreIcono));
}

const formatoPeso = new Intl.NumberFormat('es-PE', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
// Las fechas llegan como "2019-03-15" (DateOnly); se formatean en UTC para que no cambie el día
const formatoFecha = new Intl.DateTimeFormat('es-PE', { day: '2-digit', month: 'short', year: 'numeric', timeZone: 'UTC' });
const etiquetaMascotas = (n) => (n === 0 ? 'Sin mascotas' : n === 1 ? '1 mascota' : `${n} mascotas`);
const ESPECIES_SUGERIDAS = ['Perro', 'Gato', 'Conejo', 'Ave', 'Hámster'];

function hoyIso() {
  // en-CA da el formato AAAA-MM-DD con la fecha local
  return new Date().toLocaleDateString('en-CA');
}

function edad(fechaIso) {
  const [anio, mes, dia] = fechaIso.split('-').map(Number);
  const hoy = new Date();
  let meses = (hoy.getFullYear() - anio) * 12 + (hoy.getMonth() + 1 - mes);
  if (hoy.getDate() < dia) meses--;
  if (meses < 1) return 'Menos de 1 mes';
  if (meses < 12) return meses === 1 ? '1 mes' : `${meses} meses`;
  const anios = Math.floor(meses / 12);
  return anios === 1 ? '1 año' : `${anios} años`;
}

// --- Formularios y errores de la API ---

function limpiarErrores(formulario) {
  const aviso = $('.aviso', formulario);
  aviso.hidden = true;
  aviso.replaceChildren();
  formulario.querySelectorAll('.campo__error').forEach((error) => error.remove());
  formulario.querySelectorAll('[aria-invalid]').forEach((campo) => {
    campo.removeAttribute('aria-invalid');
    campo.removeAttribute('aria-describedby');
  });
}

function mostrarAviso(contenedor, mensaje) {
  const aviso = $('.aviso', contenedor);
  aviso.replaceChildren(icono('alerta'), el('span', {}, mensaje));
  aviso.hidden = false;
}

// Coloca cada error del 400 junto a su campo. Las claves llegan como "Dni", "PropietarioId" o "$.peso".
function mostrarErrores(formulario, errores) {
  const campos = [...formulario.elements].filter((campo) => campo.name);
  const sueltos = [];

  for (const [clave, mensajes] of Object.entries(errores)) {
    const nombre = clave.replace(/^\$\./, '').toLowerCase();
    const campo = campos.find((c) => c.name.toLowerCase() === nombre);
    if (!campo) {
      sueltos.push(...mensajes);
      continue;
    }
    const idError = `${campo.id}-error`;
    campo.setAttribute('aria-invalid', 'true');
    campo.setAttribute('aria-describedby', idError);
    // Basta con el primero: un nombre vacío, por ejemplo, incumple también la longitud
    campo.closest('.campo').append(el('p', { class: 'campo__error', id: idError }, mensajes[0]));
  }

  if (sueltos.length) mostrarAviso(formulario, sueltos.join(' '));
  // La API ordena los errores a su manera; el foco va al primero según el orden del formulario
  campos.find((campo) => campo.hasAttribute('aria-invalid'))?.focus();
}

async function enviarFormulario(formulario, operacion, mensajeExito) {
  limpiarErrores(formulario);
  const boton = $('[type="submit"]', formulario);
  boton.disabled = true;
  boton.textContent = 'Guardando…';

  try {
    await operacion();
    formulario.closest('dialog').close();
    avisar(mensajeExito);
    await actualizarTodo();
  } catch (error) {
    // 400: errores por campo. 409: regla de negocio incumplida. 404: el registro ya no existe.
    if (error.cuerpo?.errors) mostrarErrores(formulario, error.cuerpo.errors);
    else mostrarAviso(formulario, error.message);
  } finally {
    boton.disabled = false;
    boton.textContent = 'Guardar';
  }
}

// --- Resumen: contadores, selector de propietarios y especies ---

const estado = { propietarios: [] };

function llenarSelectorDePropietarios() {
  const selector = $('#mascota-propietario');
  const elegido = selector.value;
  selector.replaceChildren(selector.options[0], ...estado.propietarios.map((p) => el('option', { value: p.id }, p.nombres)));
  selector.value = estado.propietarios.some((p) => String(p.id) === elegido) ? elegido : '';
}

function llenarEspecies(mascotas) {
  const registradas = [...new Set(mascotas.map((m) => m.especie))].sort((a, b) => a.localeCompare(b, 'es'));

  const filtro = $('#filtro-especie');
  const elegida = filtro.value;
  filtro.replaceChildren(filtro.options[0], ...registradas.map((especie) => el('option', { value: especie }, especie)));
  filtro.value = registradas.includes(elegida) ? elegida : '';

  const sugerencias = [...new Set([...ESPECIES_SUGERIDAS, ...registradas])];
  $('#lista-especies').replaceChildren(...sugerencias.map((especie) => el('option', { value: especie })));
}

async function cargarResumen() {
  // Listados sin filtros: dan los totales de las pestañas y las opciones de los selectores
  const [propietarios, mascotas] = await Promise.all([pedir('/propietarios'), pedir('/mascotas')]);
  estado.propietarios = propietarios;
  $('#conteo-propietarios').textContent = propietarios.length;
  $('#conteo-mascotas').textContent = mascotas.length;
  llenarSelectorDePropietarios();
  llenarEspecies(mascotas);
}

// --- Propietarios ---

const filasPropietarios = $('#filas-propietarios');
const buscarPropietario = $('#buscar-propietario');
let turnoPropietarios = 0;

function pintarPropietarios(propietarios) {
  if (propietarios.length === 0) {
    const hayBusqueda = Boolean(buscarPropietario.value.trim());
    mostrarMensaje(filasPropietarios, 5,
      hayBusqueda ? 'Sin resultados' : 'Aún no hay propietarios',
      hayBusqueda ? 'Ningún propietario coincide con tu búsqueda.' : 'Registra el primero para empezar.');
    return;
  }

  filasPropietarios.replaceChildren(...propietarios.map((propietario) => el('tr', {},
    el('td', { class: 'celda-principal' },
      el('div', { class: 'titulo-fila' }, propietario.nombres),
      el('div', { class: 'subtitulo-fila' }, propietario.correo ?? 'Sin correo')),
    el('td', { class: 'dato-fijo', 'data-etiqueta': 'DNI' }, propietario.dni),
    el('td', { class: 'dato-fijo', 'data-etiqueta': 'Teléfono' }, propietario.telefono),
    el('td', { 'data-etiqueta': 'Mascotas' },
      // cantidadMascotas la calcula el MappingProfile (ForMember)
      el('button', {
        class: `enlace-conteo ${propietario.cantidadMascotas === 0 ? 'enlace-conteo--vacio' : ''}`,
        type: 'button',
        onclick: () => abrirDetalle(propietario),
      }, etiquetaMascotas(propietario.cantidadMascotas))),
    el('td', { class: 'celda-acciones' },
      el('div', { class: 'acciones-fila' },
        botonAccion('editar', `Editar a ${propietario.nombres}`, () => editarPropietario(propietario.id)),
        botonAccion('eliminar', `Eliminar a ${propietario.nombres}`, () => eliminarPropietario(propietario), 'boton-icono--peligro'))))));
}

async function cargarPropietarios() {
  const turno = ++turnoPropietarios;
  if (filasPropietarios.children.length === 0) mostrarCargando(filasPropietarios, 5);

  // La búsqueda se resuelve en la API con el GET por query string: solo números = DNI, si no = nombres
  const texto = buscarPropietario.value.trim();
  const consulta = new URLSearchParams();
  if (texto) consulta.set(/^\d+$/.test(texto) ? 'dni' : 'nombres', texto);

  try {
    const propietarios = await pedir(`/propietarios?${consulta}`);
    // Si el usuario siguió escribiendo, se descarta esta respuesta porque ya hay otra más reciente
    if (turno === turnoPropietarios) pintarPropietarios(propietarios);
  } catch (error) {
    if (turno === turnoPropietarios) {
      mostrarMensaje(filasPropietarios, 5, 'No se pudieron cargar los propietarios', error.message, { texto: 'Reintentar', alPulsar: actualizarTodo });
    }
    throw error;
  }
}

let esperaBusqueda;
buscarPropietario.addEventListener('input', () => {
  clearTimeout(esperaBusqueda);
  esperaBusqueda = setTimeout(() => cargarPropietarios().catch(() => {}), 300);
});

const formPropietario = $('#form-propietario');

function abrirFormularioPropietario(propietario = null) {
  formPropietario.reset();
  limpiarErrores(formPropietario);
  formPropietario.dataset.id = propietario?.id ?? '';
  $('#titulo-form-propietario').textContent = propietario ? 'Editar propietario' : 'Nuevo propietario';

  // PropietarioActualizarDto no tiene DNI: al editar se muestra, pero no se envía
  const campos = formPropietario.elements;
  campos.dni.disabled = Boolean(propietario);
  $('#ayuda-dni').hidden = !propietario;

  if (propietario) {
    campos.nombres.value = propietario.nombres;
    campos.dni.value = propietario.dni;
    campos.telefono.value = propietario.telefono;
    campos.correo.value = propietario.correo ?? '';
  }
  $('#dialogo-propietario').showModal();
}

async function editarPropietario(id) {
  try {
    abrirFormularioPropietario(await pedir(`/propietarios/${id}`));
  } catch (error) {
    avisar(error.message, 'error');
    if (error.codigo === 404) await actualizarTodo();
  }
}

function eliminarPropietario(propietario) {
  confirmarEliminacion(`¿Eliminar a ${propietario.nombres}?`,
    () => pedir(`/propietarios/${propietario.id}`, { metodo: 'DELETE' }), 'Propietario eliminado.');
}

formPropietario.addEventListener('submit', (evento) => {
  evento.preventDefault();
  const campos = formPropietario.elements;
  const id = formPropietario.dataset.id;
  const datos = {
    nombres: campos.nombres.value.trim(),
    telefono: campos.telefono.value.trim(),
    correo: campos.correo.value.trim() || null,
  };
  enviarFormulario(formPropietario,
    () => (id
      ? pedir(`/propietarios/${id}`, { metodo: 'PUT', datos })
      : pedir('/propietarios', { metodo: 'POST', datos: { ...datos, dni: campos.dni.value.trim() } })),
    id ? 'Propietario actualizado.' : 'Propietario registrado.');
});

// --- Mascotas de un propietario (ruta anidada) ---

let propietarioEnDetalle = null;

async function abrirDetalle(propietario) {
  propietarioEnDetalle = propietario;
  const lista = $('#lista-detalle');
  $('#subtitulo-detalle').textContent = propietario.nombres;
  lista.replaceChildren(el('li', { class: 'mensaje-lista' }, 'Cargando…'));
  $('#dialogo-detalle').showModal();

  try {
    const mascotas = await pedir(`/propietarios/${propietario.id}/mascotas`);
    lista.replaceChildren(...(mascotas.length
      ? mascotas.map((m) => el('li', {},
        el('span', { class: 'titulo-fila' }, m.nombre),
        el('span', { class: 'chip' }, m.especie),
        el('span', { class: 'texto-suave' }, m.raza ?? 'Sin raza'),
        el('span', { class: 'lista-mascotas__dato' }, `${edad(m.fechaNacimiento)} · ${formatoPeso.format(m.peso)} kg`)))
      : [el('li', { class: 'mensaje-lista' }, 'Este propietario aún no tiene mascotas.')]));
  } catch (error) {
    lista.replaceChildren(el('li', { class: 'mensaje-lista' }, error.message));
  }
}

$('#detalle-agregar').addEventListener('click', () => {
  $('#dialogo-detalle').close();
  abrirFormularioMascota(null, propietarioEnDetalle.id);
});

// --- Mascotas ---

const filasMascotas = $('#filas-mascotas');
const filtros = $('#filtros-mascotas');
let turnoMascotas = 0;

function pintarMascotas(mascotas) {
  if (mascotas.length === 0) {
    const hayFiltros = [...new FormData(filtros).values()].some((valor) => String(valor).trim());
    mostrarMensaje(filasMascotas, 6,
      hayFiltros ? 'Sin resultados' : 'Aún no hay mascotas',
      hayFiltros ? 'Ninguna mascota coincide con los filtros.' : 'Registra la primera para empezar.');
    return;
  }

  filasMascotas.replaceChildren(...mascotas.map((mascota) => el('tr', {},
    el('td', { class: 'celda-principal' },
      el('div', { class: 'titulo-fila' }, mascota.nombre),
      el('div', { class: 'subtitulo-fila' }, mascota.raza ?? 'Sin raza')),
    el('td', { 'data-etiqueta': 'Especie' }, el('span', { class: 'chip' }, mascota.especie)),
    el('td', { 'data-etiqueta': 'Nacimiento' },
      el('div', {},
        el('div', { class: 'dato-fijo' }, formatoFecha.format(new Date(mascota.fechaNacimiento))),
        el('div', { class: 'subtitulo-fila' }, edad(mascota.fechaNacimiento)))),
    el('td', { class: 'numero', 'data-etiqueta': 'Peso' }, `${formatoPeso.format(mascota.peso)} kg`),
    // propietario es el nombre que devuelve el MappingProfile (ForMember), no el objeto
    el('td', { 'data-etiqueta': 'Propietario' }, mascota.propietario),
    el('td', { class: 'celda-acciones' },
      el('div', { class: 'acciones-fila' },
        botonAccion('editar', `Editar a ${mascota.nombre}`, () => editarMascota(mascota.id)),
        botonAccion('eliminar', `Eliminar a ${mascota.nombre}`, () => eliminarMascota(mascota), 'boton-icono--peligro'))))));
}

async function cargarMascotas() {
  const turno = ++turnoMascotas;
  if (filasMascotas.children.length === 0) mostrarCargando(filasMascotas, 6);

  // Los filtros se resuelven en la API, con el GET por query string
  const consulta = new URLSearchParams();
  for (const [nombre, valor] of new FormData(filtros)) {
    if (String(valor).trim()) consulta.set(nombre, String(valor).trim());
  }

  try {
    const mascotas = await pedir(`/mascotas?${consulta}`);
    if (turno === turnoMascotas) pintarMascotas(mascotas);
  } catch (error) {
    if (turno === turnoMascotas) {
      mostrarMensaje(filasMascotas, 6, 'No se pudieron cargar las mascotas', error.message, { texto: 'Reintentar', alPulsar: actualizarTodo });
    }
    throw error;
  }
}

let esperaFiltro;
filtros.addEventListener('input', () => {
  clearTimeout(esperaFiltro);
  esperaFiltro = setTimeout(() => cargarMascotas().catch(() => {}), 300);
});
filtros.addEventListener('reset', () => {
  // El evento ocurre antes de vaciar los campos, por eso se espera al siguiente ciclo
  setTimeout(() => cargarMascotas().catch(() => {}));
});
filtros.addEventListener('submit', (evento) => evento.preventDefault());

const formMascota = $('#form-mascota');

function abrirFormularioMascota(mascota = null, propietarioId = null) {
  formMascota.reset();
  limpiarErrores(formMascota);
  formMascota.dataset.id = mascota?.id ?? '';
  $('#titulo-form-mascota').textContent = mascota ? 'Editar mascota' : 'Nueva mascota';

  const campos = formMascota.elements;
  campos.fechaNacimiento.max = hoyIso();
  campos.propietarioId.value = mascota?.propietarioId ?? propietarioId ?? '';
  if (mascota) {
    campos.nombre.value = mascota.nombre;
    campos.especie.value = mascota.especie;
    campos.raza.value = mascota.raza ?? '';
    campos.fechaNacimiento.value = mascota.fechaNacimiento;
    campos.peso.value = mascota.peso;
  }
  $('#dialogo-mascota').showModal();
}

async function editarMascota(id) {
  try {
    abrirFormularioMascota(await pedir(`/mascotas/${id}`));
  } catch (error) {
    avisar(error.message, 'error');
    if (error.codigo === 404) await actualizarTodo();
  }
}

function eliminarMascota(mascota) {
  confirmarEliminacion(`¿Eliminar a ${mascota.nombre}?`,
    () => pedir(`/mascotas/${mascota.id}`, { metodo: 'DELETE' }), 'Mascota eliminada.');
}

formMascota.addEventListener('submit', (evento) => {
  evento.preventDefault();
  limpiarErrores(formMascota);
  const campos = formMascota.elements;
  const id = formMascota.dataset.id;

  // Un número vacío no se puede enviar como JSON válido; el resto lo valida la API
  const faltantes = {};
  if (!campos.propietarioId.value) faltantes.propietarioId = ['Selecciona un propietario.'];
  if (!campos.peso.value.trim()) faltantes.peso = ['El peso es obligatorio.'];
  if (Object.keys(faltantes).length) {
    mostrarErrores(formMascota, faltantes);
    return;
  }

  const datos = {
    nombre: campos.nombre.value.trim(),
    especie: campos.especie.value.trim(),
    raza: campos.raza.value.trim() || null,
    fechaNacimiento: campos.fechaNacimiento.value || null,
    peso: Number(campos.peso.value),
    propietarioId: Number(campos.propietarioId.value),
  };
  enviarFormulario(formMascota,
    () => (id
      ? pedir(`/mascotas/${id}`, { metodo: 'PUT', datos })
      : pedir('/mascotas', { metodo: 'POST', datos })),
    id ? 'Mascota actualizada.' : 'Mascota registrada.');
});

// --- Confirmación de eliminación ---

function confirmarEliminacion(texto, operacion, mensajeExito) {
  const dialogo = $('#dialogo-confirmar');
  const boton = $('#confirmar-eliminar');
  $('.aviso', dialogo).hidden = true;
  $('#titulo-confirmar').textContent = 'Confirmar eliminación';
  $('#texto-confirmar').textContent = texto;
  $('#detalle-confirmar').hidden = false;
  $('#cancelar-eliminar').textContent = 'Cancelar';
  boton.hidden = false;

  boton.onclick = async () => {
    boton.disabled = true;
    boton.textContent = 'Eliminando…';
    try {
      await operacion();
      dialogo.close();
      avisar(mensajeExito);
      await actualizarTodo();
    } catch (error) {
      mostrarAviso(dialogo, error.message);
      // Sin conexión se puede reintentar. Si la API lo rechazó, como un propietario con mascotas (409), no.
      if (error.codigo !== 0) {
        $('#titulo-confirmar').textContent = 'No se pudo eliminar';
        $('#detalle-confirmar').hidden = true;
        $('#cancelar-eliminar').textContent = 'Entendido';
        boton.hidden = true;
      }
      if (error.codigo === 404) await actualizarTodo();
    } finally {
      boton.disabled = false;
      boton.textContent = 'Eliminar';
    }
  };

  dialogo.showModal();
}

// --- Navegación y carga inicial ---

function mostrarSeccion() {
  const actual = location.hash === '#mascotas' ? 'mascotas' : 'propietarios';
  for (const nombre of ['propietarios', 'mascotas']) {
    $(`#seccion-${nombre}`).hidden = nombre !== actual;
    const pestana = $(`.pestana[data-seccion="${nombre}"]`);
    if (nombre === actual) pestana.setAttribute('aria-current', 'page');
    else pestana.removeAttribute('aria-current');
  }
}

async function actualizarTodo() {
  try {
    await Promise.all([cargarResumen(), cargarPropietarios(), cargarMascotas()]);
    mostrarEstadoApi('activo', 'API conectada');
  } catch {
    mostrarEstadoApi('error', 'API sin respuesta');
  }
}

document.querySelectorAll('[data-cerrar]').forEach((boton) => {
  boton.addEventListener('click', () => boton.closest('dialog').close());
});
$('#nuevo-propietario').addEventListener('click', () => abrirFormularioPropietario());
$('#nueva-mascota').addEventListener('click', () => abrirFormularioMascota());
window.addEventListener('hashchange', mostrarSeccion);

mostrarSeccion();
actualizarTodo();
