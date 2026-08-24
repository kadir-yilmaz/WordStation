/**
 * Word Detail Modal Logic
 * Shared between Word and Synonym views.
 */

window.showWordDetail = function(en, tr, example) {
    const enEl = document.getElementById('detailWordEn');
    const trEl = document.getElementById('detailWordTr');
    const exEl = document.getElementById('detailWordExample');

    if (enEl) enEl.textContent = en || '';
    if (trEl) trEl.textContent = tr || '';
    
    if(exEl) {
        if(example && example.trim() !== '') {
            exEl.textContent = example;
            exEl.parentElement.style.display = 'block';
        } else {
            exEl.parentElement.style.display = 'none';
        }
    }
    
    const modalEl = document.getElementById('wordDetailModal');
    if(modalEl) {
        const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
        modal.show();
    }
};

window.showWordDetailFromElement = function(element) {
    const en = element.getAttribute('data-en');
    const tr = element.getAttribute('data-tr');
    const example = element.getAttribute('data-example');
    showWordDetail(en, tr, example);
};

// Detail modal açıkken klavyeden '.' basıldığında telaffuz çal
document.addEventListener('keydown', function(e) {
    if ((e.target.tagName === 'INPUT' && e.target.type !== 'range') || e.target.tagName === 'TEXTAREA') return;

    const modalEl = document.getElementById('wordDetailModal');
    if (modalEl && modalEl.classList.contains('show')) {
        const key = e.key;
        if (key === '.' || e.code === 'Period' || e.code === 'NumpadDecimal') {
            e.preventDefault();
            const detailWordEn = document.getElementById('detailWordEn')?.textContent?.trim();
            if (detailWordEn && window.speakWord) {
                window.speakWord(detailWordEn);
            }
        }
    }
});
