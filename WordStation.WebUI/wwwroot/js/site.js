/* =====================================================
   Pending Form Data - localStorage ile form verisi koruma
   
   Akış:
   1. Form submit öncesi → veriyi localStorage'a kaydet
   2. Normal form.submit() (sıfır gecikme)
   3. Token expired → Login sayfasına redirect
   4. Login sayfasında → ReturnUrl'i localStorage'dan düzelt
   5. Login sonrası → Word sayfası açılır → veri geri yüklenir
   ===================================================== */

window.savePendingFormData = function (form) {
    const formData = new FormData(form);
    const data = {};
    formData.forEach((value, key) => {
        if (key !== '__RequestVerificationToken') {
            data[key] = value;
        }
    });

    // Form tipini belirle
    const action = form.action || '';
    let formType = 'unknown';
    if (action.includes('UpdateWord')) formType = 'update';
    else if (action.includes('CreateWord')) formType = 'create';
    else if (action.includes('DeleteWord')) formType = 'delete';

    // Mevcut sayfa URL'ini returnUrl olarak kaydet (restore=1 ile)
    const currentUrl = window.location.pathname + window.location.search;
    const separator = currentUrl.includes('?') ? '&' : '?';
    const returnUrl = currentUrl + separator + 'restore=1';

    const pending = {
        formType: formType,
        data: data,
        returnUrl: returnUrl,
        timestamp: Date.now()
    };

    localStorage.setItem('ws_pendingFormData', JSON.stringify(pending));
};

/* =====================================================
   Form Submit Wrapper - Sıfır Gecikme
   
   fetch YOK. Sadece:
   1. Veriyi localStorage'a kaydet (anlık)
   2. Normal form.submit() (tarayıcı hızı)
   ===================================================== */
window.submitFormAjax = function (form) {
    window.savePendingFormData(form);
    form.submit();
};

/* =====================================================
   Pending Form Data Restore (Word sayfasında çağrılır)
   ===================================================== */
window.restorePendingFormData = function () {
    const urlParams = new URLSearchParams(window.location.search);

    // restore=1 yoksa → normal sayfa yüklemesi → stale veriyi temizle
    if (!urlParams.has('restore')) {
        localStorage.removeItem('ws_pendingFormData');
        return;
    }

    // URL'den restore parametresini temizle (tekrar tetiklenmesin)
    urlParams.delete('restore');
    const cleanUrl = window.location.pathname + (urlParams.toString() ? '?' + urlParams.toString() : '');
    history.replaceState(null, '', cleanUrl);

    const raw = localStorage.getItem('ws_pendingFormData');
    if (!raw) return;

    try {
        const pending = JSON.parse(raw);

        // 5 dakikadan eski veriyi temizle
        if (Date.now() - pending.timestamp > 5 * 60 * 1000) {
            localStorage.removeItem('ws_pendingFormData');
            return;
        }

        const data = pending.data;

        if (pending.formType === 'update') {
            const modal = document.getElementById('updateWordModal');
            if (modal) {
                const setVal = (id, val) => {
                    const el = document.getElementById(id);
                    if (el && val !== undefined) el.value = val;
                };
                setVal('updateId', data.Id);
                setVal('updateEn', data.En);
                setVal('updateTr', data.Tr);
                setVal('updateExample', data.Example);
                setVal('updateListNameHidden', data.ListName);

                setTimeout(() => {
                    const bsModal = new bootstrap.Modal(modal);
                    bsModal.show();
                }, 400);
            }
        } else if (pending.formType === 'create') {
            const modal = document.getElementById('createWordModal');
            if (modal) {
                const form = modal.querySelector('form');
                if (form) {
                    const setInput = (name, val) => {
                        const el = form.querySelector(`[name="${name}"]`);
                        if (el && val !== undefined) el.value = val;
                    };
                    setInput('En', data.En);
                    setInput('Tr', data.Tr);
                    setInput('Example', data.Example);
                }

                setTimeout(() => {
                    const bsModal = new bootstrap.Modal(modal);
                    bsModal.show();
                }, 400);
            }
        } else if (pending.formType === 'delete') {
            // Delete için otomatik silme yapmıyoruz (tehlikeli)
            // Kullanıcıya bilgi veriyoruz
            const wordEn = data.wordEn || 'unknown';
            setTimeout(() => {
                const alertDiv = document.createElement('div');
                alertDiv.className = 'alert alert-warning alert-dismissible fade show';
                alertDiv.style.cssText = 'position:fixed;top:80px;left:50%;transform:translateX(-50%);z-index:9999;min-width:350px;border-radius:12px;';
                alertDiv.innerHTML = `
                    <i class="bi bi-exclamation-triangle me-2"></i>
                    <strong>"${wordEn}"</strong> kelimesini silmek üzereydiniz. Oturumunuz dolduğu için işlem tamamlanamadı. Lütfen tekrar deneyin.
                    <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                `;
                document.body.appendChild(alertDiv);
                setTimeout(() => alertDiv.remove(), 8000);
            }, 500);
        }

        localStorage.removeItem('ws_pendingFormData');
    } catch (e) {
        localStorage.removeItem('ws_pendingFormData');
    }
};

/* =====================================================
   DOMContentLoaded - Login sayfasında ReturnUrl düzeltme
   
   Token expire olunca cookie middleware şu ReturnUrl'i üretir:
   /Account/Login?ReturnUrl=%2FWord%2FUpdateWord
   
   Login sonrası GET /Word/UpdateWord'e gider → 405 hatası!
   
   Biz localStorage'daki doğru URL'i (Word/Index) koyuyoruz.
   ===================================================== */
document.addEventListener('DOMContentLoaded', () => {
    const raw = localStorage.getItem('ws_pendingFormData');
    if (!raw) return;

    try {
        const pending = JSON.parse(raw);

        // 5 dakikadan eski veriyi yoksay
        if (Date.now() - pending.timestamp > 5 * 60 * 1000) {
            localStorage.removeItem('ws_pendingFormData');
            return;
        }

        // Login sayfasındaysak → ReturnUrl'i düzelt
        const returnUrlInput = document.querySelector('input[name="ReturnUrl"]');
        if (returnUrlInput && pending.returnUrl) {
            returnUrlInput.value = pending.returnUrl;
        }
    } catch (e) {
        // Parse hatası → temizle
    }
});
