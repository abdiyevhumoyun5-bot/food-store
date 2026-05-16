/**
 * Food Store — Savat (Cart) JavaScript
 * Serverga POST so'rov yuboradi va savat sonini yangilaydi.
 */

// CSRF tokenni olish
function getAntiforgeryToken() {
    const el = document.querySelector('input[name="__RequestVerificationToken"]');
    return el ? el.value : '';
}

// POST funksiyasi
async function cartPost(url, data) {
    const token = getAntiforgeryToken();
    const body  = new URLSearchParams({ ...data, __RequestVerificationToken: token });
    const res   = await fetch(url, {
        method:  'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body:    body.toString()
    });
    return res.json();
}

// Savatga qo'shish
function addToCart(productId, qty = 1) {
    cartPost('/Cart/Add', { productId, quantity: qty }).then(d => {
        if (d.success) {
            updateCartCount(d.count);
            showToast('✅ Savatga qo\'shildi!', 'success');
        }
    }).catch(() => showToast('❌ Xato yuz berdi', 'danger'));
}

// Miqdor yangilash
function updateCart(productId, quantity) {
    cartPost('/Cart/Update', { productId, quantity }).then(d => {
        if (d.success) {
            updateCartCount(d.count);
            if (quantity <= 0) {
                const row = document.getElementById('row-' + productId);
                if (row) row.remove();
            } else {
                const qtyEl = document.getElementById('qty-' + productId);
                if (qtyEl) qtyEl.textContent = quantity;
                location.reload(); // Jami narxni yangilash uchun
            }
        }
    });
}

// Savatdan o'chirish
function removeFromCart(productId) {
    cartPost('/Cart/Remove', { productId }).then(d => {
        if (d.success) {
            updateCartCount(d.count);
            const row = document.getElementById('row-' + productId);
            if (row) row.remove();
            if (d.count === 0) location.reload();
        }
    });
}

// Navbar savat sonini yangilash
function updateCartCount(count) {
    const el = document.getElementById('cartCount');
    if (!el) return;
    if (count > 0) {
        el.textContent = count;
        el.style.display = '';
    } else {
        el.style.display = 'none';
    }
}

// Sevimlilar
function toggleFavorite(productId, btn) {
    cartPost('/Cabinet/ToggleFavorite', { productId }).then(d => {
        if (d.success) {
            const icon = btn.querySelector('i');
            if (d.added) {
                icon.className = 'bi bi-heart-fill';
                btn.classList.remove('btn-success');
                btn.classList.add('btn-danger');
            } else {
                icon.className = 'bi bi-heart';
                btn.classList.remove('btn-danger');
                btn.classList.add('btn-success');
            }
        }
    }).catch(() => {
        // Login bo'lmagan foydalanuvchi
        window.location.href = '/Account/Login';
    });
}

// Toast xabari (Bootstrap)
function showToast(msg, type = 'success') {
    // Mavjud toast konteynerini topish yoki yaratish
    let container = document.getElementById('toastContainer');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toastContainer';
        container.style.cssText = 'position:fixed;bottom:20px;right:20px;z-index:9999;display:flex;flex-direction:column;gap:8px';
        document.body.appendChild(container);
    }

    const toast = document.createElement('div');
    toast.className = `alert alert-${type} shadow-sm py-2 px-3 mb-0`;
    toast.style.cssText = 'min-width:200px;animation:fadeIn 0.3s;border-radius:10px';
    toast.innerHTML = msg;
    container.appendChild(toast);

    setTimeout(() => { toast.style.opacity = '0'; toast.style.transition = 'opacity 0.4s'; }, 2500);
    setTimeout(() => toast.remove(), 3000);
}

// CSRF token uchun yashirin forma (agar sahifada yo'q bo'lsa)
document.addEventListener('DOMContentLoaded', function() {
    if (!document.querySelector('input[name="__RequestVerificationToken"]')) {
        fetch('/Cart/Count'); // Server bilan bog'lanib token olish uchun
    }
});
