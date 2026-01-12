// Global State
let currentIndex = 0;
let touchStartX = 0;
let isDragging = false;
let usedRandomIndices = new Set(); // Kullanılmış random indexleri takip et

// DOM Elements
const els = {
    tableView: document.getElementById('tableView'),
    flashcardView: document.getElementById('flashcardView'),
    toggleBtn: document.getElementById('toggleViewBtn'),
    toggleIcon: document.getElementById('toggleViewIcon'),
    flashcard: document.getElementById('flashcard'),
    container: document.getElementById('flashcardContainer'),
    word: document.getElementById('flashcardWord'),
    translation: document.getElementById('flashcardTranslation'),
    progress: document.getElementById('flashcardProgress'),
    exampleText: document.getElementById('flashcardExampleText'),
    slider: document.getElementById('interactiveSlider'),
    sliderTrack: document.getElementById('sliderTrack'),
    sliderHandle: document.getElementById('sliderHandle'),
    prevBtn: document.getElementById('flashcardPrev'),
    nextBtn: document.getElementById('flashcardNext'),
    randomBtn: document.getElementById('flashcardRandom'),
    searchInput: document.getElementById('searchInput'),
    btnUpdate: document.getElementById('flashcardUpdateBtn'),
    btnDelete: document.getElementById('flashcardDeleteBtn')
};

/* =====================================================
   Storage Key Generator (Liste bazlı kayıt)
   ===================================================== */
function getStorageKey(suffix, ignoreSearch = false) {
    const listName = window.selectedList || 'default';
    const searchTerm = window.searchTermValue || '';

    // viewMode için arama terimini yoksay (liste bazlı)
    if (ignoreSearch || !searchTerm) {
        return `ws_${listName}_${suffix}`;
    }
    // wordIndex için arama bazlı key kullan
    return `ws_${listName}_search_${searchTerm}_${suffix}`;
}

/* =====================================================
   Anti-Forgery Token
   ===================================================== */
function getAntiForgeryToken() {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenInput ? tokenInput.value : null;
}

/* =====================================================
   Delete Word
   ===================================================== */
function deleteWord(id, listName, wordEn, searchTerm) {
    if (confirm(`"${wordEn}" kelimesini silmek istediğinize emin misiniz?`)) {
        const isCardView = !els.flashcardView.classList.contains('d-none');
        const form = document.createElement('form');
        form.method = 'POST';
        form.action = '/Word/DeleteWord';

        const inputs = [
            { name: 'id', value: id },
            { name: 'listName', value: listName },
            { name: 'SearchTerm', value: searchTerm || '' },
            { name: 'wordEn', value: wordEn || '' }
        ];

        const token = getAntiForgeryToken();
        if (token) inputs.push({ name: '__RequestVerificationToken', value: token });

        inputs.forEach(i => {
            const inp = document.createElement('input');
            inp.type = 'hidden';
            inp.name = i.name;
            inp.value = i.value;
            form.appendChild(inp);
        });

        // Table view'da scroll pozisyonunu koru
        if (!els.tableView.classList.contains('d-none')) {
            sessionStorage.setItem('scrollTarget', 'preserve');
            sessionStorage.setItem('scrollPosition', window.scrollY.toString());
        }

        // Flashcard view'da mevcut index'i kaydet
        // Silinen kelime sonuncuysa bir öncekine git
        if (isCardView) {
            let indexToSave = currentIndex;
            // Eğer son kelimedeysek ve birden fazla kelime varsa, bir öncekine git
            if (window.wordsData && currentIndex >= window.wordsData.length - 1 && currentIndex > 0) {
                indexToSave = currentIndex - 1;
            }
            sessionStorage.setItem(getStorageKey('wordIndex'), indexToSave.toString());
        }

        document.body.appendChild(form);
        form.submit();
    }
}
window.deleteWord = deleteWord;

/* =====================================================
   View Toggle
   ===================================================== */
function toggleView(mode) {
    const isCurrentlyFlashcard = !els.flashcardView.classList.contains('d-none');
    let targetMode = mode;

    if (!targetMode) {
        targetMode = isCurrentlyFlashcard ? 'table' : 'flashcard';
    }

    if (targetMode === 'table') {
        els.flashcardView.classList.add('d-none');
        els.tableView.classList.remove('d-none');

        // PC'de scroll aktif - flashcard-active kaldır
        document.documentElement.classList.remove('flashcard-active');

        if (els.toggleIcon) {
            els.toggleIcon.classList.remove('bi-table');
            els.toggleIcon.classList.add('bi-credit-card-2-front');
        }

        sessionStorage.setItem(getStorageKey('viewMode', true), 'table');
    } else {
        els.tableView.classList.add('d-none');
        els.flashcardView.classList.remove('d-none');

        // PC'de scroll devre dışı - flashcard-active ekle
        document.documentElement.classList.add('flashcard-active');

        if (els.toggleIcon) {
            els.toggleIcon.classList.remove('bi-credit-card-2-front');
            els.toggleIcon.classList.add('bi-table');
        }

        sessionStorage.setItem(getStorageKey('viewMode', true), 'flashcard');

        if (window.wordsData?.length > 0 && !els.flashcard.dataset.init) {
            els.flashcard.dataset.init = 'true';
            showWord(currentIndex);
        }
    }
}

els.toggleBtn?.addEventListener('click', () => toggleView());

/* =====================================================
   Show Word
   ===================================================== */
function showWord(index, opts = {}) {
    if (!window.wordsData || !window.wordsData.length) return;
    if (index < 0 || index >= window.wordsData.length) return;

    currentIndex = index;

    if (!opts.skipStorage) {
        sessionStorage.setItem(getStorageKey('wordIndex'), currentIndex);
    }

    const data = window.wordsData[index];

    if (!opts.keepFlip) {
        els.flashcard.classList.remove('flipped');
    }

    els.word.textContent = data.en || data.En;
    els.translation.textContent = data.tr || data.Tr;

    if (!opts.light) {
        const example = data.example || data.Example;
        if (els.exampleText) {
            if (example) {
                els.exampleText.textContent = `${example}`;
                els.exampleText.classList.remove('text-white-50', 'fs-6');
                els.exampleText.classList.add('text-white');
            } else {
                els.exampleText.textContent = "No example sentence available.";
                els.exampleText.classList.add('text-white-50', 'fs-6');
                els.exampleText.classList.remove('text-white');
            }
        }

        if (els.btnUpdate) {
            els.btnUpdate.dataset.id = data.id || data.Id;
            els.btnUpdate.dataset.en = data.en || data.En;
            els.btnUpdate.dataset.tr = data.tr || data.Tr;
            els.btnUpdate.dataset.example = example || '';
            els.btnUpdate.dataset.listname = data.listName || data.ListName;
        }

        if (els.btnDelete) {
            els.btnDelete.onclick = () =>
                deleteWord(
                    data.id || data.Id,
                    data.listName || data.ListName,
                    data.en || data.En,
                    window.searchTermValue
                );
        }
    }

    const total = window.wordsData.length;
    const percent = total > 1 ? (index / (total - 1)) * 100 : 0;

    if (els.progress) els.progress.textContent = `${index + 1} / ${total}`;

    if (!opts.skipSliderUpdate) {
        if (els.sliderTrack) els.sliderTrack.style.width = `${percent}%`;
        if (els.sliderHandle) els.sliderHandle.style.left = `calc(${percent}% - 10px)`;
    }

    if (els.prevBtn) els.prevBtn.disabled = (index === 0);
    if (els.nextBtn) els.nextBtn.disabled = (index === total - 1);
}

/* =====================================================
   Smooth Slider Logic
   ===================================================== */
let sliderRect = null;
let rafId = null;
let pendingX = 0;
let dragIndex = 0;

function scheduleDragUpdate(clientX) {
    pendingX = clientX;
    if (rafId) return;

    rafId = requestAnimationFrame(() => {
        rafId = null;
        if (!sliderRect) sliderRect = els.slider.getBoundingClientRect();

        let p = (pendingX - sliderRect.left) / sliderRect.width;
        p = Math.max(0, Math.min(1, p));

        const total = window.wordsData.length;
        const idx = Math.round(p * (total - 1));
        dragIndex = idx;

        if (els.sliderTrack) els.sliderTrack.style.width = `${p * 100}%`;
        if (els.sliderHandle) els.sliderHandle.style.left = `calc(${p * 100}% - 10px)`;
        if (els.progress) els.progress.textContent = `${idx + 1} / ${total}`;

        if (idx !== currentIndex) {
            showWord(idx, {
                light: true,
                skipStorage: true,
                skipSliderUpdate: true,
                keepFlip: true
            });
        }
    });
}

function commitSlider() {
    showWord(dragIndex);
    sliderRect = null;
}

if (els.slider) {
    els.slider.addEventListener('mousedown', (e) => {
        if (!window.wordsData || window.wordsData.length <= 1) return;
        isDragging = true;
        sliderRect = els.slider.getBoundingClientRect();
        els.slider.classList.add('dragging');
        scheduleDragUpdate(e.clientX);
    });

    document.addEventListener('mousemove', (e) => {
        if (!isDragging) return;
        e.preventDefault();
        scheduleDragUpdate(e.clientX);
    });

    document.addEventListener('mouseup', () => {
        if (!isDragging) return;
        isDragging = false;
        els.slider.classList.remove('dragging');
        commitSlider();
    });

    els.slider.addEventListener('touchstart', (e) => {
        if (!window.wordsData || window.wordsData.length <= 1) return;
        isDragging = true;
        sliderRect = els.slider.getBoundingClientRect();
        els.slider.classList.add('dragging');
        scheduleDragUpdate(e.touches[0].clientX);
    }, { passive: false });

    document.addEventListener('touchmove', (e) => {
        if (!isDragging) return;
        e.preventDefault();
        scheduleDragUpdate(e.touches[0].clientX);
    }, { passive: false });

    document.addEventListener('touchend', () => {
        if (!isDragging) return;
        isDragging = false;
        els.slider.classList.remove('dragging');
        commitSlider();
    });
}

/* =====================================================
   Card Interaction
   ===================================================== */
els.flashcard?.addEventListener('click', () => els.flashcard.classList.toggle('flipped'));

/* =====================================================
   Keyboard Controls
   ===================================================== */
document.addEventListener('keydown', (e) => {
    if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA') return;
    if (els.flashcardView.classList.contains('d-none')) return;

    const key = e.key;

    if (key === 'ArrowLeft') {
        if (currentIndex > 0) showWord(currentIndex - 1);
    }
    else if (key === 'ArrowRight') {
        if (currentIndex < window.wordsData.length - 1) showWord(currentIndex + 1);
    }
    else if (key === 'ArrowUp') {
        e.preventDefault();
        els.flashcard.classList.toggle('flipped');
    }
    else if (key === 'ArrowDown') {
        e.preventDefault();
        const nextIndex = getRandomUnusedIndex();
        if (nextIndex !== -1) showWord(nextIndex);
    }
});

/* =====================================================
   Button Listeners
   ===================================================== */
els.prevBtn?.addEventListener('click', () => showWord(currentIndex - 1));
els.nextBtn?.addEventListener('click', () => showWord(currentIndex + 1));

// Random kelime seçme - tekrarsız
function getRandomUnusedIndex() {
    if (!window.wordsData || window.wordsData.length === 0) return -1;

    const totalWords = window.wordsData.length;

    // Mevcut kelimeyi kullanıldı olarak işaretle
    usedRandomIndices.add(currentIndex);

    // Tüm kelimeler kullanıldıysa sıfırla (mevcut hariç)
    if (usedRandomIndices.size >= totalWords) {
        usedRandomIndices.clear();
        usedRandomIndices.add(currentIndex); // Mevcut kelimeyi hemen tekrar gösterme
    }

    // Kullanılmamış indexleri bul
    const unusedIndices = [];
    for (let i = 0; i < totalWords; i++) {
        if (!usedRandomIndices.has(i)) {
            unusedIndices.push(i);
        }
    }

    // Kullanılmamış indexlerden rastgele seç
    if (unusedIndices.length === 0) return currentIndex;

    const randomIndex = unusedIndices[Math.floor(Math.random() * unusedIndices.length)];
    usedRandomIndices.add(randomIndex);

    return randomIndex;
}

els.randomBtn?.addEventListener('click', () => {
    const nextIndex = getRandomUnusedIndex();
    if (nextIndex !== -1) {
        showWord(nextIndex);
    }
});

/* =====================================================
   Search Logic
   ===================================================== */
// Enter tuşu ile form submit
els.searchInput?.addEventListener('keypress', (e) => {
    if (e.key === 'Enter') {
        e.target.closest('form').submit();
    }
});

/* =====================================================
   Modal Data Fillers
   ===================================================== */
document.getElementById('updateWordModal')?.addEventListener('show.bs.modal', function (e) {
    const btn = e.relatedTarget;
    this.querySelector('#updateId').value = btn.dataset.id;
    this.querySelector('#updateEn').value = btn.dataset.en;
    this.querySelector('#updateTr').value = btn.dataset.tr;
    this.querySelector('#updateExample').value = btn.dataset.example;
    this.querySelector('#updateListNameHidden').value = btn.dataset.listname;
});



/* =====================================================
   Init
   ===================================================== */
document.addEventListener('DOMContentLoaded', () => {

    const savedView = sessionStorage.getItem(getStorageKey('viewMode', true));
    let savedIndex = parseInt(sessionStorage.getItem(getStorageKey('wordIndex'))) || 0;

    // Index kontrolü: Kelime sayısından fazlaysa 0'a çek
    if (window.wordsData && savedIndex >= window.wordsData.length) {
        savedIndex = 0;
    }

    // View mode ayarla
    if (savedView === 'table') {
        toggleView('table');
    } else {
        // Flashcard default - PC'de scroll devre dışı
        document.documentElement.classList.add('flashcard-active');

        if (window.wordsData?.length > 0) {
            els.flashcard.dataset.init = 'true';
            showWord(savedIndex);
        }

        if (els.toggleIcon) {
            els.toggleIcon.classList.remove('bi-credit-card-2-front');
            els.toggleIcon.classList.add('bi-table');
        }
    }

    // Scroll Logic (Table View için)
    const scrollId = sessionStorage.getItem('scrollTarget');
    if (scrollId && !els.tableView.classList.contains('d-none')) {
        if (scrollId === 'bottom') {
            window.scrollTo(0, document.body.scrollHeight);
        } else if (scrollId === 'preserve') {
            // Kaydedilen pozisyona geri dön
            const savedPosition = parseInt(sessionStorage.getItem('scrollPosition')) || 0;
            window.scrollTo(0, savedPosition);
            sessionStorage.removeItem('scrollPosition');
        } else {
            const row = document.getElementById(scrollId);
            if (row) row.scrollIntoView({ block: 'center' });
        }
        sessionStorage.removeItem('scrollTarget');
    }
});

/* =====================================================
   Form Submit Handlers (Scroll Target)
   ===================================================== */
document.querySelector('#createWordModal form')?.addEventListener('submit', () => {
    if (!els.tableView.classList.contains('d-none')) {
        // Mevcut scroll pozisyonunu kaydet, sayfayı olduğu yerde tut
        sessionStorage.setItem('scrollTarget', 'preserve');
        sessionStorage.setItem('scrollPosition', window.scrollY.toString());
    }
});

document.querySelector('#updateWordModal form')?.addEventListener('submit', () => {
    if (!els.tableView.classList.contains('d-none')) {
        // Mevcut scroll pozisyonunu kaydet, sayfayı olduğu yerde tut
        sessionStorage.setItem('scrollTarget', 'preserve');
        sessionStorage.setItem('scrollPosition', window.scrollY.toString());
    }
});

/* =====================================================
   Scroll To Top Button
   ===================================================== */
const scrollTopBtn = document.getElementById("scrollTopBtn");
window.addEventListener("scroll", () => {
    if (window.scrollY > 300) scrollTopBtn.style.display = "block";
    else scrollTopBtn.style.display = "none";
});
scrollTopBtn.addEventListener("click", () => {
    window.scrollTo({ top: 0, behavior: "smooth" });
});