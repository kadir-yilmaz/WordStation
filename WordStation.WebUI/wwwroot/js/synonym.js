/**
 * Synonyms page logic
 * - Türkçe karakter duyarsız arama (ı/i, ğ/g, ü/u, ş/s, ö/o, ç/c)
 * - Arama hem İngilizce kelimelerde hem de Türkçe anlamlarda yapılır
 * - Arama sırasında eş anlamlısı olmayan (grupsuz) eşleşen kelimeler ayrı bölümde gösterilir
 * - Grupları kelime sayısı veya alfabetik olarak sıralama
 */
document.addEventListener('DOMContentLoaded', function () {
    const searchInput = document.getElementById('synonymsSearchInput');
    if (!searchInput) return;

    const clearBtn = document.getElementById('synSearchClear');
    const emptyClearBtn = document.getElementById('synEmptyClear');
    const subtitle = document.getElementById('synSubtitle');

    const groupsList = document.getElementById('groupsList');
    const singlesList = document.getElementById('singlesList');
    const groupsHeader = document.getElementById('groupsHeader');
    const groupsHeaderBadge = document.getElementById('groupsHeaderBadge');
    const singlesSection = document.getElementById('singlesSection');
    const singlesSubtitle = document.getElementById('singlesSubtitle');
    const singlesHeaderBadge = document.getElementById('singlesHeaderBadge');
    const emptyNoGroups = document.getElementById('emptyNoGroups');
    const emptyNoResults = document.getElementById('emptyNoResults');

    const groupCards = Array.from(groupsList.querySelectorAll('.syn-card'));
    const singleCards = Array.from(singlesList.querySelectorAll('.syn-card'));

    let sortOption = 'countDesc';

    function normalize(str) {
        return (str || '')
            .toLocaleLowerCase('tr')
            .replace(/ı/g, 'i')
            .replace(/\u0307/g, '')
            .replace(/ğ/g, 'g')
            .replace(/ü/g, 'u')
            .replace(/ş/g, 's')
            .replace(/ö/g, 'o')
            .replace(/ç/g, 'c')
            .trim();
    }

    // Her kart için arama metnini bir kere normalize et
    [...groupCards, ...singleCards].forEach(card => {
        card._search = (card.dataset.search || '').split('|').map(normalize);
    });

    function matches(card, q) {
        if (!q) return true;
        return card._search.some(s => s.includes(q));
    }

    const collator = new Intl.Collator('tr', { sensitivity: 'base' });

    function compare(a, b) {
        const countA = parseInt(a.dataset.count, 10) || 0;
        const countB = parseInt(b.dataset.count, 10) || 0;
        const trA = a.dataset.tr || '';
        const trB = b.dataset.tr || '';
        switch (sortOption) {
            case 'countAsc':
                return (countA - countB) || collator.compare(trA, trB);
            case 'alphaAsc':
                return collator.compare(trA, trB) || (countB - countA);
            case 'alphaDesc':
                return collator.compare(trB, trA) || (countB - countA);
            case 'countDesc':
            default:
                return (countB - countA) || collator.compare(trA, trB);
        }
    }

    function applySort() {
        groupCards.sort(compare).forEach(c => groupsList.appendChild(c));
        singleCards.sort(compare).forEach(c => singlesList.appendChild(c));
    }

    function render() {
        const raw = searchInput.value.trim();
        const q = normalize(raw);
        const isSearching = q.length > 0;

        clearBtn.hidden = !isSearching;

        let groupCount = 0;
        groupCards.forEach(card => {
            const show = matches(card, q);
            card.hidden = !show;
            if (show) groupCount++;
        });

        let singleGroupCount = 0;
        let singleWordCount = 0;
        singleCards.forEach(card => {
            const show = isSearching && matches(card, q);
            card.hidden = !show;
            if (show) {
                singleGroupCount++;
                singleWordCount += parseInt(card.dataset.count, 10) || 0;
            }
        });

        // Bölüm başlıkları
        groupsHeader.hidden = !(isSearching && groupCount > 0 && singleGroupCount > 0);
        groupsHeaderBadge.textContent = `${groupCount} groups`;

        singlesSection.hidden = singleGroupCount === 0;
        singlesHeaderBadge.textContent = `${singleWordCount} words`;
        singlesSubtitle.textContent = groupCount === 0
            ? 'No synonym group found, but the word exists in your dictionary:'
            : 'Other matches that have no synonym group';

        // Boş durumlar
        emptyNoGroups.hidden = isSearching || groupCards.length > 0;
        emptyNoResults.hidden = !(isSearching && groupCount === 0 && singleGroupCount === 0);

        // Alt başlık
        if (!isSearching) {
            subtitle.textContent = subtitle.dataset.default;
        } else if (groupCount > 0 && singleWordCount > 0) {
            subtitle.textContent = `${groupCount} groups • ${singleWordCount} single words found`;
        } else if (groupCount > 0) {
            subtitle.textContent = `${groupCount} synonym groups found`;
        } else if (singleWordCount > 0) {
            subtitle.textContent = `${singleWordCount} single words found (no synonyms)`;
        } else {
            subtitle.textContent = 'No results found';
        }
    }

    function clearSearch() {
        searchInput.value = '';
        render();
        searchInput.focus();
    }

    searchInput.addEventListener('input', render);
    searchInput.addEventListener('keydown', e => { if (e.key === 'Escape') clearSearch(); });
    clearBtn.addEventListener('click', clearSearch);
    emptyClearBtn.addEventListener('click', clearSearch);

    document.querySelectorAll('.syn-sort-menu [data-sort]').forEach(item => {
        item.addEventListener('click', function () {
            sortOption = this.dataset.sort;
            document.querySelectorAll('.syn-sort-menu [data-sort]').forEach(i => i.classList.remove('active'));
            this.classList.add('active');
            applySort();
        });
    });

    applySort();
    render();
});
