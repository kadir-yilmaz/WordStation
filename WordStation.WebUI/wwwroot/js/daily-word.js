/**
 * Günlük Kelimeler (Daily Words) - Study Page JavaScript
 * 
 * Handles:
 * - Tab switching (tümü / çalışılan)
 * - Search filtering (EN + TR)
 * - Add to daily (→) via MVC AJAX proxy
 * - Remove from daily (←) via MVC AJAX proxy
 * - Complete word (✓) via MVC AJAX proxy
 * - Animations
 */

(function () {
    'use strict';

    const config = window.dailyWordConfig;
    if (!config) return;

    // ===== DOM References =====

    const allWordsList = document.getElementById('allWordsList');
    const completedWordsList = document.getElementById('completedWordsList');
    const dailyWordsList = document.getElementById('dailyWordsList');
    const searchBox = document.getElementById('searchBox');
    const searchInput = document.getElementById('searchInput');
    const searchClear = document.getElementById('searchClear');

    // Search mode & language toggle buttons
    const searchModeToggle = document.getElementById('searchModeToggle');
    const searchLangToggle = document.getElementById('searchLangToggle');

    // Search state
    let currentSearchMode = 'starts'; // 'starts' or 'contains'
    let currentSearchLang = 'en';     // 'en' or 'tr'

    // Count elements
    const allCountEl = document.getElementById('allCount');
    const completedCountEl = document.getElementById('completedCount');
    const dailyCountEl = document.getElementById('dailyCount');
    const completedCountBadgeEl = document.getElementById('completedCountBadge'); // Might be removed in HTML, keeping for safety
    const completeAllBtn = document.getElementById('completeAllBtn');

    // ===== AJAX Helper (calls MVC Controller, not WebAPI directly) =====
    async function mvcPost(url, body) {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            },
            body: JSON.stringify(body)
        });

        if (response.status === 401) {
            // Session expired
            const modal = document.getElementById('sessionExpiredModal');
            if (modal) {
                const bsModal = new bootstrap.Modal(modal);
                bsModal.show();
            }
            throw new Error('Unauthorized');
        }

        if (!response.ok) {
            throw new Error(`Error: ${response.status}`);
        }

        return await response.json();
    }



    // ===== Search =====

    if (searchInput) {
        searchInput.addEventListener('input', (e) => {
            filterWords(e.target.value);
        });
    }

    if (searchClear) {
        searchClear.addEventListener('click', () => {
            searchInput.value = '';
            filterWords('');
            searchInput.focus();
        });
    }

    // Search Mode Toggle (StartsWith / Contains)
    if (searchModeToggle) {
        searchModeToggle.addEventListener('click', () => {
            const modeIcon = searchModeToggle.querySelector('i');
            if (currentSearchMode === 'starts') {
                currentSearchMode = 'contains';
                searchModeToggle.classList.add('active');
                modeIcon?.classList.replace('bi-text-left', 'bi-text-center');
                searchModeToggle.title = 'Mode: Contains';
            } else {
                currentSearchMode = 'starts';
                searchModeToggle.classList.remove('active');
                modeIcon?.classList.replace('bi-text-center', 'bi-text-left');
                searchModeToggle.title = 'Mode: Starts with';
            }
            if (searchInput && searchInput.value) filterWords(searchInput.value);
            searchInput?.focus();
        });
    }

    // Language Toggle (EN / TR)
    if (searchLangToggle) {
        searchLangToggle.addEventListener('click', () => {
            if (currentSearchLang === 'en') {
                currentSearchLang = 'tr';
                searchLangToggle.classList.add('active');
                searchLangToggle.textContent = 'TR';
                searchLangToggle.title = 'Lang: Türkçe';
            } else {
                currentSearchLang = 'en';
                searchLangToggle.classList.remove('active');
                searchLangToggle.textContent = 'EN';
                searchLangToggle.title = 'Lang: English';
            }
            if (searchInput && searchInput.value) filterWords(searchInput.value);
            searchInput?.focus();
        });
    }

    function filterWords(term) {
        const query = term.toLowerCase().trim();

        // Filter both lists
        [allWordsList, completedWordsList].forEach(list => {
            if (!list) return;
            const rows = list.querySelectorAll('.dw-word-row');
            rows.forEach(row => {
                if (!query) {
                    row.style.display = '';
                    return;
                }
                const fieldValue = row.getAttribute(`data-${currentSearchLang}`) || '';
                const matches = currentSearchMode === 'starts'
                    ? fieldValue.startsWith(query)
                    : fieldValue.includes(query);
                row.style.display = matches ? '' : 'none';
            });
        });
    }

    // ===== Add to Daily (→) =====
    function handleAddToDaily(e) {
        const btn = e.target.closest('.dw-btn-send');
        if (!btn) return;

        const wordId = parseInt(btn.getAttribute('data-word-id'));
        const row = btn.closest('.dw-word-row');
        if (!row || !wordId) return;

        // Disable button to prevent double-click
        btn.disabled = true;

        mvcPost(config.addToDailyUrl, {
            listName: config.listName,
            wordIds: [wordId]
        }).then(result => {
            // Animate out from left panel
            row.classList.add('dw-animate-out');

            setTimeout(() => {
                const en = row.querySelector('.dw-word-en')?.textContent || '';
                const tr = row.querySelector('.dw-word-tr')?.textContent || '';
                const example = row.getAttribute('data-example') || '';
                row.remove();

                // Add to daily list (right panel)
                addWordToDailyPanel(wordId, en, tr, example);

                // Remove empty state from daily if present
                const emptyState = document.getElementById('dailyEmptyState');
                if (emptyState) emptyState.remove();

                updateCounts();
            }, 300);
        }).catch(err => {
            console.error('AddToDaily error:', err);
            btn.disabled = false;
        });
    }

    function addWordToDailyPanel(wordId, en, tr, example) {
        const row = document.createElement('div');
        row.className = 'dw-word-row dw-daily-row dw-animate-in-right';
        row.setAttribute('data-word-id', wordId);
        row.setAttribute('data-en', en.toLowerCase());
        row.setAttribute('data-tr', tr.toLowerCase());
        row.setAttribute('data-example', example);
        row.innerHTML = `
            <button class="dw-btn-send dw-btn-send-left" title="Geri gönder" data-word-id="${wordId}">
                <i class="bi bi-arrow-left"></i>
            </button>
            <div class="dw-word-content" onclick="window.showWordDetailFromElement(this.closest('.dw-word-row'))" style="cursor: pointer;" title="Detay">
                <span class="dw-word-en">${escapeHtml(en)}</span>
                <span class="dw-word-tr">${escapeHtml(tr)}</span>
            </div>
            <div class="dw-row-actions">
                <button class="dw-btn-complete" title="Çalışıldı olarak işaretle" data-word-id="${wordId}">
                    <i class="bi bi-check-lg"></i>
                </button>
            </div>
        `;
        dailyWordsList.appendChild(row);
    }

    // ===== Remove from Daily (←) =====
    function handleRemoveFromDaily(e) {
        const btn = e.target.closest('.dw-btn-send-left');
        if (!btn) return;

        const wordId = parseInt(btn.getAttribute('data-word-id'));
        const row = btn.closest('.dw-word-row');
        if (!row || !wordId) return;

        btn.disabled = true;

        mvcPost(config.removeFromDailyUrl, {
            listName: config.listName,
            wordIds: [wordId]
        }).then(result => {
            // Animate out from right panel
            row.classList.add('dw-animate-out');

            setTimeout(() => {
                const en = row.querySelector('.dw-word-en')?.textContent || '';
                const tr = row.querySelector('.dw-word-tr')?.textContent || '';
                const example = row.getAttribute('data-example') || '';
                row.remove();

                // Add back to left panel (all words list)
                addWordToAllPanel(wordId, en, tr, example);

                // Show empty state if no daily words left
                checkDailyEmpty();
                updateCounts();
            }, 300);
        }).catch(err => {
            console.error('RemoveFromDaily error:', err);
            btn.disabled = false;
        });
    }

    function addWordToAllPanel(wordId, en, tr, example) {
        // Remove empty state if present
        const emptyState = allWordsList.querySelector('.dw-empty-list');
        if (emptyState) emptyState.remove();

        const row = document.createElement('div');
        row.className = 'dw-word-row dw-animate-in-left';
        row.setAttribute('data-word-id', wordId);
        row.setAttribute('data-en', en.toLowerCase());
        row.setAttribute('data-tr', tr.toLowerCase());
        row.setAttribute('data-example', example);
        row.innerHTML = `
            <div class="dw-word-content" onclick="window.showWordDetailFromElement(this.closest('.dw-word-row'))" style="cursor: pointer;" title="Detay">
                <span class="dw-word-en">${escapeHtml(en)}</span>
                <span class="dw-word-tr">${escapeHtml(tr)}</span>
            </div>
            <div class="dw-row-actions">
                <button class="dw-btn-send dw-btn-send-right" title="Günlüğe ekle" data-word-id="${wordId}">
                    <i class="bi bi-arrow-right"></i>
                </button>
            </div>
        `;
        allWordsList.appendChild(row);
    }

    // ===== Complete Word (✓) =====
    function handleCompleteWord(e) {
        const btn = e.target.closest('.dw-btn-complete');
        if (!btn) return;

        const wordId = parseInt(btn.getAttribute('data-word-id'));
        const row = btn.closest('.dw-word-row');
        if (!row || !wordId) return;

        btn.disabled = true;

        mvcPost(config.completeWordUrl, {
            listName: config.listName,
            wordId: wordId
        }).then(result => {
            // Animate completion
            row.classList.add('dw-animate-complete');

            setTimeout(() => {
                const en = row.querySelector('.dw-word-en')?.textContent || '';
                const tr = row.querySelector('.dw-word-tr')?.textContent || '';
                const example = row.getAttribute('data-example') || '';
                row.remove();

                // Add to completed list
                addWordToCompletedPanel(wordId, en, tr, example);
                checkDailyEmpty();
                updateCounts();
            }, 400);
        }).catch(err => {
            console.error('CompleteWord error:', err);
            btn.disabled = false;
        });
    }

    function addWordToCompletedPanel(wordId, en, tr, example) {
        // Remove empty state if present
        const emptyState = document.getElementById('completedEmptyState');
        if (emptyState) emptyState.remove();

        const row = document.createElement('div');
        row.className = 'dw-word-row dw-word-completed';
        row.setAttribute('data-word-id', wordId);
        row.setAttribute('data-en', en.toLowerCase());
        row.setAttribute('data-tr', tr.toLowerCase());
        row.setAttribute('data-example', example);
        row.innerHTML = `
            <button class="dw-btn-send dw-btn-send-left" title="Günlüğe geri al" data-word-id="${wordId}">
                <i class="bi bi-arrow-left"></i>
            </button>
            <div class="dw-word-content" onclick="window.showWordDetailFromElement(this.closest('.dw-word-row'))" style="cursor: pointer;" title="Detay">
                <div class="d-flex align-items-center gap-2 me-auto" style="min-width: 0;">
                    <span class="dw-word-en text-truncate">${escapeHtml(en)}</span>
                </div>
                <span class="dw-word-tr">${escapeHtml(tr)}</span>
            </div>
        `;
        
        let lastCard = completedWordsList.querySelectorAll('.day-group-card');
        let cardToAppend = lastCard.length > 0 ? lastCard[lastCard.length - 1] : null;

        if (cardToAppend) {
            const body = cardToAppend.querySelector('.day-group-body');
            if (body) {
                body.appendChild(row);
            }
        } else {
            // No cards exist yet, create Day 1 card
            const card = document.createElement('div');
            card.className = 'day-group-card';
            card.style = 'border: 1px solid rgba(255,255,255,0.1); border-radius: 12px; margin-bottom: 16px; padding: 12px; background: rgba(0,0,0,0.2); position: relative;';
            card.innerHTML = `
                <div class="day-group-header" style="text-align: right; color: rgba(255,255,255,0.5); font-weight: 500; font-size: 0.85rem; margin-bottom: 8px;">
                    Day 1
                </div>
                <div class="day-group-body"></div>
            `;
            card.querySelector('.day-group-body').appendChild(row);
            completedWordsList.appendChild(card);
        }
    }

    // ===== Complete All Words =====
    if (completeAllBtn) {
        completeAllBtn.addEventListener('click', async () => {
            const rows = dailyWordsList.querySelectorAll('.dw-word-row');
            if (rows.length === 0) return;

            completeAllBtn.disabled = true;
            const originalHtml = completeAllBtn.innerHTML;
            completeAllBtn.innerHTML = '<i class="bi bi-hourglass-split"></i> İşleniyor...';

            try {
                const wordIdsToProcess = [];
                const rowsData = [];
                
                // Get all IDs and original DOM elements first
                Array.from(rows).forEach(row => {
                    const wordId = parseInt(row.getAttribute('data-word-id'));
                    wordIdsToProcess.push(wordId);
                    
                    rowsData.push({
                        row: row,
                        wordId: wordId,
                        en: row.querySelector('.dw-word-en')?.textContent || '',
                        tr: row.querySelector('.dw-word-tr')?.textContent || '',
                        example: row.getAttribute('data-example') || ''
                    });
                });

                // Send 1 BULK request to the server instead of multiple requests
                await mvcPost(config.completeWordsUrl, {
                    listName: config.listName,
                    wordIds: wordIdsToProcess
                });

                // If successful, animate all rows to completed list
                rowsData.forEach(data => {
                    data.row.classList.add('dw-animate-complete');
                    setTimeout(() => {
                        data.row.remove();
                        addWordToCompletedPanel(data.wordId, data.en, data.tr, data.example);
                    }, 400);
                });
                
                setTimeout(() => {
                    checkDailyEmpty();
                    updateCounts();
                }, 450);
                
            } catch (err) {
                console.error('CompleteAll error:', err);
                alert('Tümünü tamamlarken bir hata oluştu. Sayfayı yenileyip tekrar deneyin.');
            } finally {
                completeAllBtn.disabled = false;
                completeAllBtn.innerHTML = originalHtml;
            }
        });
    }

    // ===== Event Delegation =====
    if (allWordsList) {
        allWordsList.addEventListener('click', handleAddToDaily);
    }

    if (completedWordsList) {
        completedWordsList.addEventListener('click', (e) => {
            handleAddToDaily(e);
            
            // Eğer tamamlananlardan gönderildiyse empty state kontrolü (handleAddToDaily async çalışıyor, bu yüzden timeout veya count logic içinde yapmak daha doğru ama şimdilik burada kalabilir. Aslında en doğrusu count updates içinde halletmektir.)
        });
    }

    if (dailyWordsList) {
        dailyWordsList.addEventListener('click', (e) => {
            handleRemoveFromDaily(e);
            handleCompleteWord(e);
        });
    }

    // ===== Count Updates =====
    function updateCounts() {
        if (allCountEl) {
            allCountEl.textContent = allWordsList.querySelectorAll('.dw-word-row').length;
        }
        if (completedCountEl) {
            const count = completedWordsList.querySelectorAll('.dw-word-row').length;
            completedCountEl.textContent = count;
        }
        if (dailyCountEl) {
            dailyCountEl.textContent = dailyWordsList.querySelectorAll('.dw-word-row').length;
        }
        if (completedCountBadgeEl) {
            completedCountBadgeEl.textContent = completedWordsList.querySelectorAll('.dw-word-row').length;
        }

        checkCompletedEmpty();
    }

    function checkCompletedEmpty() {
        const completedRows = completedWordsList.querySelectorAll('.dw-word-row').length;
        if (completedRows === 0 && !document.getElementById('completedEmptyState')) {
            const emptyDiv = document.createElement('div');
            emptyDiv.className = 'dw-empty-list';
            emptyDiv.id = 'completedEmptyState';
            emptyDiv.innerHTML = `
                <i class="bi bi-hourglass"></i>
                <p>Henüz çalışılmış kelime yok</p>
            `;
            completedWordsList.appendChild(emptyDiv);
        }
    }

    function checkDailyEmpty() {
        const dailyRows = dailyWordsList.querySelectorAll('.dw-word-row').length;
        if (dailyRows === 0 && !document.getElementById('dailyEmptyState')) {
            const emptyDiv = document.createElement('div');
            emptyDiv.className = 'dw-empty-list';
            emptyDiv.id = 'dailyEmptyState';
            emptyDiv.innerHTML = `
                <i class="bi bi-arrow-left-circle"></i>
                <p>Soldaki listeden kelime ekleyin</p>
            `;
            dailyWordsList.appendChild(emptyDiv);
        }
    }

    // ===== Utility =====
    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

})();
