/**
 * Food Store — Umumiy JavaScript
 */

// Alertlarni 5 soniyadan keyin yashirish
document.addEventListener('DOMContentLoaded', function () {
    setTimeout(function () {
        document.querySelectorAll('.alert-dismissible').forEach(function (el) {
            var bsAlert = bootstrap.Alert.getOrCreateInstance(el);
            if (bsAlert) bsAlert.close();
        });
    }, 5000);
});

// Savat sonini yangilash
function updateCartCount(count) {
    var el = document.getElementById('cartCount');
    if (!el) return;
    if (count > 0) {
        el.textContent = count;
        el.style.display = '';
    } else {
        el.style.display = 'none';
    }
}
