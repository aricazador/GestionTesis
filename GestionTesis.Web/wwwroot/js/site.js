// 1) GLightbox: al hacer clic en una miniatura con clase .glightbox se ve en grande
document.addEventListener('DOMContentLoaded', () => {
    if (window.GLightbox) GLightbox({ selector: '.glightbox', touchNavigation: true, loop: true });

    // 2) Botones "Editar": llenan el modal de edición con los datos de la fila (data-valores = JSON)
    document.querySelectorAll('[data-editar]').forEach(boton => {
        boton.addEventListener('click', () => {
            const form = document.querySelector(boton.dataset.editar);
            const valores = JSON.parse(boton.dataset.valores);
            for (const [nombre, valor] of Object.entries(valores)) {
                const campo = form.querySelector(`[name="${nombre}"]`);
                if (campo) campo.value = valor ?? '';
            }
            limpiarErrores(form);
        });
    });

    // 3) Botones "Nuevo": abren el modal limpio
    document.querySelectorAll('[data-nuevo]').forEach(boton => {
        boton.addEventListener('click', () => {
            const form = document.querySelector(boton.dataset.nuevo);
            form.reset();
            limpiarErrores(form);
        });
    });

    // 4) Confirmación antes de eliminar
    document.querySelectorAll('form[data-confirmar]').forEach(form => {
        form.addEventListener('submit', e => {
            if (!confirm(form.dataset.confirmar)) e.preventDefault();
        });
    });

    // 5) Si el servidor devolvió errores, reabrimos el modal que corresponde
    const modalAAbrir = document.body.dataset.abrirModal;
    if (modalAAbrir) {
        const el = document.getElementById(modalAAbrir);
        if (el) bootstrap.Modal.getOrCreateInstance(el).show();
    }
});

function limpiarErrores(form) {
    form.querySelectorAll('.field-validation-error').forEach(e => {
        e.textContent = '';
        e.classList.remove('field-validation-error');
        e.classList.add('field-validation-valid');
    });
    form.querySelectorAll('.input-validation-error').forEach(e => e.classList.remove('input-validation-error'));
    form.querySelectorAll('.validation-summary-errors').forEach(s => {
        s.classList.remove('validation-summary-errors');
        s.classList.add('validation-summary-valid');
        const ul = s.querySelector('ul');
        if (ul) ul.innerHTML = '';
    });
}
