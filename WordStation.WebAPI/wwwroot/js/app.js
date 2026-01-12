const userId = 'i64QzXr0JNNWReEqDpCn4RL5FqU2';

let words = [];
let currentIndex = 0;
let flashcardMode = false;

// DOM Elemanları
const toggleBtn = document.getElementById('toggleMode');
const listSelect = document.getElementById('listSelect');
const searchInput = document.getElementById('searchInput');
const tableBody = document.getElementById('tableBody');
const card = document.getElementById('card');
const front = document.getElementById('front');
const back = document.getElementById('back');
const counter = document.getElementById('counter');
const slider = document.getElementById('slider');
const example = document.getElementById('example');

// Event Listeners
toggleBtn?.addEventListener('click', toggleFlashcardMode);
listSelect?.addEventListener('change', fetchWords);
searchInput?.addEventListener('input', debounce(fetchWords, 300));
document.addEventListener('keydown', handleKeyboard);
slider?.addEventListener('input', handleSliderInput);
slider?.addEventListener('change', handleSliderChange);

// Debounce (gecikmeli arama)
function debounce(func, wait) {
    let timeout;
    return function (...args) {
        clearTimeout(timeout);
        timeout = setTimeout(() => func(...args), wait);
    };
}

// Liste adlarını çek
async function fetchListNames() {
    const url = `http://wsapi.runasp.net/api/words/listNames?userId=${userId}`;
    try {
        const response = await fetch(url);
        if (!response.ok) throw new Error("Liste adları alınamadı");
        const listNames = await response.json();
        populateListSelect(listNames);
    } catch (error) {
        console.error("Liste Adı API Hatası:", error);
        listSelect.innerHTML = '<option value="">Listeler yüklenemedi</option>';
        listSelect.disabled = true;
    }
}

// Select menüsünü doldur
function populateListSelect(listNames) {
    listSelect.innerHTML = '';
    if (!listNames || listNames.length === 0) {
        listSelect.innerHTML = '<option value="">Liste bulunamadı</option>';
        listSelect.disabled = true;
        return;
    }

    listSelect.disabled = false;
    listNames.forEach(name => {
        const option = document.createElement('option');
        option.value = name;
        option.textContent = name;
        listSelect.appendChild(option);
    });

    fetchWords(); // ilk liste otomatik yüklenir
}

// Kelime verilerini çek
async function fetchWords() {
    const listName = listSelect.value;
    if (!listName) {
        tableBody.innerHTML = '<tr><td colspan="4" style="text-align: center; padding: 2rem;">Lütfen bir kelime listesi seçin.</td></tr>';
        words = [];
        updateUI();
        return;
    }

    const searchTerm = searchInput.value.trim();
    const listNameUpper = listName.toUpperCase();
    const url = searchTerm
        ? `http://wsapi.runasp.net/api/words/search?userId=${userId}&listName=${encodeURIComponent(listNameUpper)}&en=${encodeURIComponent(searchTerm)}`
        : `http://wsapi.runasp.net/api/words?userId=${userId}&listName=${encodeURIComponent(listNameUpper)}`;

    showLoading();

    try {
        const response = await fetch(url);
        if (!response.ok) throw new Error("Kelimeler alınamadı");
        words = await response.json() || [];
        currentIndex = 0;
        updateUI();
    } catch (error) {
        console.error("API Hatası:", error);
        showError('Kelimeler yüklenirken hata oluştu.');
        words = [];
        updateUI();
    }
}

// UI Güncellemeleri
function updateUI() {
    updateTable();
    updateFlashcard();
    updateSlider();
}

function updateTable() {
    if (!listSelect.value) return;

    if (words.length === 0 && searchInput.value.trim() !== '') {
        tableBody.innerHTML = '<tr><td colspan="4" style="text-align: center; padding: 2rem;">Sonuç bulunamadı.</td></tr>';
        return;
    }

    if (words.length === 0) {
        tableBody.innerHTML = '<tr><td colspan="4" style="text-align: center; padding: 2rem;">Bu listede hiç kelime yok.</td></tr>';
        return;
    }

    tableBody.innerHTML = words.map((word, index) => `
        <tr>
            <td>${index + 1}</td>
            <td>${word.en}</td>
            <td>${word.tr}</td>
            <td>${(word.example || '').replace(/\n/g, '<br>')}</td>
        </tr>
    `).join('');
}

function updateFlashcard() {
    if (words.length === 0) {
        front.textContent = '-';
        back.textContent = '-';
        example.textContent = 'Kelime listesi boş';
        counter.textContent = '0 / 0';
        card.classList.remove('flipped');
        return;
    }

    const word = words[currentIndex];
    front.textContent = word.en;
    back.textContent = word.tr;
    example.innerHTML = word.example ? word.example.replace(/\n/g, '<br>') : 'Örnek cümle yok';
    counter.textContent = `${currentIndex + 1} / ${words.length}`;
    card.classList.remove('flipped');
}

function updateSlider() {
    if (words.length === 0) {
        slider.max = 0;
        slider.value = 0;
        slider.disabled = true;
        updateSliderVisual();
        return;
    }

    slider.disabled = false;
    slider.max = words.length - 1;
    slider.value = currentIndex;
    updateSliderVisual();
}

function updateSliderVisual() {
    const percentage = words.length <= 1 ? 0 : (currentIndex / (words.length - 1)) * 100;
    slider.style.setProperty('--slider-percentage', `${percentage}%`);
}

// Loading ve Error
function showLoading() {
    tableBody.innerHTML = `
        <tr>
            <td colspan="4" class="loading">
                <div class="spinner"></div>
                <p>Yükleniyor...</p>
            </td>
        </tr>
    `;
}

function showError(message) {
    tableBody.innerHTML = `<tr><td colspan="4" style="text-align:center; color:red;">${message}</td></tr>`;
}

// Flashcard Kontrolleri
function flipCard() {
    card.classList.toggle('flipped');
}

function nextWord() {
    if (currentIndex < words.length - 1) {
        currentIndex++;
        updateFlashcard();
        updateSlider();
    }
}

function prevWord() {
    if (currentIndex > 0) {
        currentIndex--;
        updateFlashcard();
        updateSlider();
    }
}

function randomWord() {
    if (words.length <= 1) return;
    let newIndex;
    do {
        newIndex = Math.floor(Math.random() * words.length);
    } while (newIndex === currentIndex);
    currentIndex = newIndex;
    updateFlashcard();
    updateSlider();
}

function handleSliderInput(e) {
    const newIndex = parseInt(e.target.value);
    if (newIndex !== currentIndex) {
        currentIndex = newIndex;
        updateFlashcard();
        updateSliderVisual();
    }
}

function handleSliderChange(e) {
    currentIndex = parseInt(e.target.value);
    updateFlashcard();
    updateSliderVisual();
}

function toggleFlashcardMode() {
    flashcardMode = !flashcardMode;
    document.body.classList.toggle('flashcard-mode', flashcardMode);
    toggleBtn.textContent = flashcardMode ? 'Tablo Modu' : 'Flashcard Modu';
    if (flashcardMode) {
        updateFlashcard();
        updateSlider();
    }
}

function handleKeyboard(e) {
    if (!flashcardMode) return;

    if (e.key === 'ArrowRight') {
        e.preventDefault();
        nextWord();
    } else if (e.key === 'ArrowLeft') {
        e.preventDefault();
        prevWord();
    } else if (e.key === ' ') {
        e.preventDefault();
        flipCard();
    }
}

// Uygulama başlangıcı
document.addEventListener('DOMContentLoaded', () => {
    fetchListNames();
});
