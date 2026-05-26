// VehicleRent Pro – site.js

// ── Sidebar Toggle ────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    const toggle  = document.getElementById('sidebarToggle');
    const wrapper = document.getElementById('wrapper');

    if (toggle && wrapper) {
        toggle.addEventListener('click', () => {
            wrapper.classList.toggle('sidebar-collapsed');
            localStorage.setItem('sidebarCollapsed', wrapper.classList.contains('sidebar-collapsed'));
        });

        // Restore preference
        if (localStorage.getItem('sidebarCollapsed') === 'true') {
            wrapper.classList.add('sidebar-collapsed');
        }
    }

    // ── Auto-dismiss alerts after 5 s ────────────────────────
    document.querySelectorAll('.alert-dismissible').forEach(el => {
        setTimeout(() => {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(el);
            bsAlert?.close();
        }, 5000);
    });

    // ── Confirmation for delete buttons ──────────────────────
    document.querySelectorAll('[data-confirm]').forEach(btn => {
        btn.addEventListener('click', e => {
            if (!confirm(btn.dataset.confirm)) e.preventDefault();
        });
    });

    // ── Plate number auto-uppercase ───────────────────────────
    document.querySelectorAll('input[name="PlateNumber"]').forEach(inp => {
        inp.addEventListener('input', () => { inp.value = inp.value.toUpperCase(); });
    });

    // ── Tooltip init ──────────────────────────────────────────
    document.querySelectorAll('[title]').forEach(el => {
        new bootstrap.Tooltip(el, { trigger: 'hover' });
    });
});
