// Глобальные переменные
let currentImages = [];
let selectedImageId = null;
let currentZoom = 1;
let fitMode = true;

// DOM элементы
const fileListEl = document.getElementById('fileList');
const imageViewer = document.getElementById('imageViewer');
const imageDescriptionEl = document.getElementById('imageDescription');
const zoomLevelEl = document.getElementById('zoomLevel');

// API Calls
const API_URL = '/api/images';

async function loadImages() {
    try {
        const response = await fetch(API_URL);
        currentImages = await response.json();
        renderFileList();

        // Если есть выбранное изображение, обновляем его
        if (selectedImageId) {
            const stillExists = currentImages.find(img => img.id === selectedImageId);
            if (!stillExists) {
                selectedImageId = null;
                clearViewer();
            } else {
                renderCurrentImage();
            }
        }
    } catch (error) {
        console.error('Ошибка загрузки:', error);
    }
}

function renderFileList() {
    if (currentImages.length === 0) {
        fileListEl.innerHTML = '<div class="empty-state">Нет загруженных изображений</div>';
        return;
    }

    fileListEl.innerHTML = currentImages.map(img => `
        <div class="file-item ${selectedImageId === img.id ? 'selected' : ''}" data-id="${img.id}">
            <div class="file-title">${escapeHtml(img.title || img.fileName)}</div>
            <div class="file-date">${formatDate(img.uploadDate)}</div>
            <div class="file-actions">
                <button class="edit-btn" data-id="${img.id}" data-action="edit">✏️ Редактировать</button>
                <button class="delete-btn" data-id="${img.id}" data-action="delete">🗑️ Удалить</button>
            </div>
        </div>
    `).join('');

    // Добавляем обработчики
    document.querySelectorAll('.file-item').forEach(item => {
        item.addEventListener('click', (e) => {
            if (!e.target.classList.contains('edit-btn') && !e.target.classList.contains('delete-btn')) {
                const id = parseInt(item.dataset.id);
                selectImage(id);
            }
        });
    });

    document.querySelectorAll('.edit-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.stopPropagation();
            const id = parseInt(btn.dataset.id);
            openEditModal(id);
        });
    });

    document.querySelectorAll('.delete-btn').forEach(btn => {
        btn.addEventListener('click', async (e) => {
            e.stopPropagation();
            const id = parseInt(btn.dataset.id);
            if (confirm('Удалить это изображение?')) {
                await deleteImage(id);
            }
        });
    });
}

async function selectImage(id) {
    selectedImageId = id;
    renderFileList();
    await renderCurrentImage();
}

async function renderCurrentImage() {
    if (!selectedImageId) return;

    const image = currentImages.find(img => img.id === selectedImageId);
    if (!image) return;

    // Отображаем описание
    if (image.description) {
        imageDescriptionEl.innerHTML = `<div>${escapeHtml(image.description)}</div>`;
    } else {
        imageDescriptionEl.innerHTML = '<div class="description-placeholder">Краткое описание не указано</div>';
    }

    // Отображаем изображение
    imageViewer.innerHTML = '';
    const img = document.createElement('img');
    img.src = image.filePath;
    img.alt = image.title || image.fileName;

    const container = document.createElement('div');
    container.className = fitMode ? 'fit-container' : 'image-container';
    container.appendChild(img);

    if (!fitMode) {
        img.style.transform = `scale(${currentZoom})`;
        img.style.transformOrigin = 'center';
    }

    imageViewer.appendChild(container);

    // Ждем загрузки изображения для настройки скролла
    img.onload = () => {
        if (!fitMode) {
            // Центрируем изображение в области просмотра
            const viewerRect = imageViewer.getBoundingClientRect();
            const imgRect = img.getBoundingClientRect();
            imageViewer.scrollLeft = (imgRect.width - viewerRect.width) / 2;
            imageViewer.scrollTop = (imgRect.height - viewerRect.height) / 2;
        }
    };
}

function clearViewer() {
    imageViewer.innerHTML = '<div class="no-selection">Выберите изображение из списка</div>';
    imageDescriptionEl.innerHTML = '<div class="description-placeholder">Краткое описание</div>';
}

// Загрузка изображения
document.getElementById('uploadBtn').addEventListener('click', () => {
    document.getElementById('uploadModal').style.display = 'flex';
});

document.getElementById('closeModal').addEventListener('click', () => {
    document.getElementById('uploadModal').style.display = 'none';
});

document.getElementById('cancelUpload').addEventListener('click', () => {
    document.getElementById('uploadModal').style.display = 'none';
});

document.getElementById('uploadForm').addEventListener('submit', async (e) => {
    e.preventDefault();

    const file = document.getElementById('imageFile').files[0];
    if (!file) return;

    const formData = new FormData();
    formData.append('file', file);
    formData.append('title', document.getElementById('imageTitle').value);
    formData.append('description', document.getElementById('imageDescriptionInput').value);

    try {
        const response = await fetch(API_URL, {
            method: 'POST',
            body: formData
        });

        if (response.ok) {
            document.getElementById('uploadModal').style.display = 'none';
            document.getElementById('uploadForm').reset();
            await loadImages();
        } else {
            alert('Ошибка загрузки');
        }
    } catch (error) {
        console.error('Ошибка:', error);
    }
});

// Редактирование
async function openEditModal(id) {
    const image = currentImages.find(img => img.id === id);
    if (!image) return;

    document.getElementById('editId').value = id;
    document.getElementById('editTitle').value = image.title || '';
    document.getElementById('editDescription').value = image.description || '';
    document.getElementById('editModal').style.display = 'flex';
}

document.getElementById('closeEditModal').addEventListener('click', () => {
    document.getElementById('editModal').style.display = 'none';
});

document.getElementById('cancelEdit').addEventListener('click', () => {
    document.getElementById('editModal').style.display = 'none';
});

document.getElementById('editForm').addEventListener('submit', async (e) => {
    e.preventDefault();

    const id = parseInt(document.getElementById('editId').value);
    const title = document.getElementById('editTitle').value;
    const description = document.getElementById('editDescription').value;

    try {
        const response = await fetch(`${API_URL}/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ title, description })
        });

        if (response.ok) {
            document.getElementById('editModal').style.display = 'none';
            await loadImages();
        } else {
            alert('Ошибка обновления');
        }
    } catch (error) {
        console.error('Ошибка:', error);
    }
});

// Удаление
async function deleteImage(id) {
    try {
        const response = await fetch(`${API_URL}/${id}`, { method: 'DELETE' });
        if (response.ok) {
            if (selectedImageId === id) {
                selectedImageId = null;
                clearViewer();
            }
            await loadImages();
        } else {
            alert('Ошибка удаления');
        }
    } catch (error) {
        console.error('Ошибка:', error);
    }
}

// Управление масштабом (дополнительная часть)
function updateZoom() {
    if (!selectedImageId || fitMode) return;

    const img = document.querySelector('.image-viewer img');
    if (img) {
        img.style.transform = `scale(${currentZoom})`;
        zoomLevelEl.textContent = `${Math.round(currentZoom * 100)}%`;
    }
}

document.getElementById('zoomInBtn').addEventListener('click', () => {
    if (fitMode) {
        fitMode = false;
        document.querySelector('.image-viewer').classList.remove('fit-container');
        renderCurrentImage();
    }
    currentZoom = Math.min(currentZoom + 0.1, 3);
    updateZoom();
});

document.getElementById('zoomOutBtn').addEventListener('click', () => {
    if (fitMode) {
        fitMode = false;
        document.querySelector('.image-viewer').classList.remove('fit-container');
        renderCurrentImage();
    }
    currentZoom = Math.max(currentZoom - 0.1, 0.3);
    updateZoom();
});

document.getElementById('fitBtn').addEventListener('click', () => {
    fitMode = true;
    currentZoom = 1;
    renderCurrentImage();
});

document.getElementById('resetZoomBtn').addEventListener('click', () => {
    if (fitMode) {
        fitMode = false;
        renderCurrentImage();
    }
    currentZoom = 1;
    updateZoom();
});

// Вспомогательные функции
function escapeHtml(str) {
    if (!str) return '';
    return str.replace(/[&<>]/g, function (m) {
        if (m === '&') return '&amp;';
        if (m === '<') return '&lt;';
        if (m === '>') return '&gt;';
        return m;
    });
}

function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('ru-RU') + ' ' + date.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });
}

// Инициализация
loadImages();