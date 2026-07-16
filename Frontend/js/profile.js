import {apiServerAddress} from "./config.js";
import {getAuthToken, buildShortUrl, formatDate, removeMilliseconds} from "./common.js";

const toastEl = document.getElementById("app-toast");
const toastBody = document.getElementById("toast-body");
const toast = new bootstrap.Toast(toastEl, { delay: 2200 });

const urlsContainer = {
    self: document.getElementById("urls-container"),
    loadingIndicator: document.getElementById("urls-loading"),
    errorIndicator: document.getElementById("urls-error"),
    emptyIndicator: document.getElementById("urls-empty"),
    urlsTable: document.getElementById("urls-table-wrap"),
    urlsTableBody: document.getElementById("urls-tbody"),
    childs: document.getElementById("urls-container").children
};

const urlsShowExpiredCheckbox = document.getElementById("urls-show-expired");

const editUrlModalEl = document.getElementById("edit-url-modal");
const editUrlForm = document.getElementById("edit-url-form");
const editShortCodeInput = document.getElementById("edit-short-code");
const editOriginalUrlInput = document.getElementById("edit-original-url");
const editExpiryDateInput = document.getElementById("edit-expiry-date");
const editUrlSaveBtn = document.getElementById("edit-url-save-btn");
const editUrlModal = new bootstrap.Modal(editUrlModalEl);

function showToast(message) {
    toastBody.textContent = message;
    toast.show();
}

let urlsCacheActive = null;
let urlsCacheAll = null;
let editOriginalValues = null;

function showUrlsContainerElement(element) {
    if (element.classList.contains("d-none")) {
        element.classList.remove("d-none");
    }

    for (const item of urlsContainer.childs) {
        if (item !== element) {
            if (!item.classList.contains("d-none")) {
                item.classList.add("d-none");
            }
        }
    }
}

function deleteUrlFromUI(shortCode) {
    const urlToDelete = document.getElementById(shortCode);
    urlToDelete.remove();
}

async function deleteUrl(shortCode) {
    try {
        const response = await fetch(apiServerAddress + `/links/${shortCode}`, {
            method: "DELETE",
            headers: {
                "Authorization": `Bearer ${getAuthToken()}`
            }
        });
        if (!response.ok) {
            console.error(response);
            showToast("Ошибка при удалении ссылки");
            return;
        }
        deleteUrlFromUI(shortCode);
        if (urlsCacheActive) {
            urlsCacheActive = urlsCacheActive.filter(u => u.shortCode !== shortCode);
        }
        if (urlsCacheAll) urlsCacheAll = urlsCacheAll.filter(u => u.shortCode !== shortCode);
        if (urlsContainer.urlsTableBody.rows.length === 0) {
            showUrlsContainerElement(urlsContainer.emptyIndicator);
        }

        showToast("Ссылка успешно удалена");
    }
    catch (error) {
        console.error(error);
        alert("Ошибка при удалении ссылки");
    }
}

async function copyUrl(text) {
    if (navigator.clipboard && window.isSecureContext) {
        await navigator.clipboard.writeText(text);
    }
    else {
        const tmp = document.createElement("textarea");
        tmp.value = text;
        tmp.setAttribute("readonly", "");
        tmp.style.position = "absolute";
        tmp.style.left = "-9999px";
        document.body.appendChild(tmp);
        tmp.select();
        document.execCommand("copy");
        document.body.removeChild(tmp);
    }

    showToast("Ссылка скопирована");
}

function toDatetimeLocalValue(isoString) {
    if (!isoString) return "";
    const d = new Date(isoString);
    if (Number.isNaN(d.getTime())) return "";
    const pad = (n) => String(n).padStart(2, "0");
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

function openEditModal(shortCode, originalUrl, expiresAt) {
    editShortCodeInput.value = shortCode;
    editOriginalUrlInput.value = originalUrl;
    editExpiryDateInput.value = toDatetimeLocalValue(expiresAt);
    editOriginalValues = {
        originalUrl: originalUrl,
        expiresAt: editExpiryDateInput.value
    };
    editUrlForm.classList.remove("was-validated");
    editUrlModal.show();
}

function updateUrlRow(tr, originalUrl, expiresAt) {
    const originalLink = tr.cells[0].querySelector("a");
    originalLink.href = originalUrl;
    originalLink.textContent = originalUrl;
    tr.dataset.expiresAt = expiresAt || "";
    tr.cells[3].textContent = expiresAt ? formatDate(expiresAt) : "Нет срока жизни";
}

function updateUrlCaches(shortCode, originalUrl, expiresAt) {
    const update = (cache) => {
        if (!cache) return;
        const idx = cache.findIndex(u => u.shortCode === shortCode);
        if (idx !== -1) {
            cache[idx] = { ...cache[idx], originalUrl, expiresAt };
        }
    };
    update(urlsCacheActive);
    update(urlsCacheAll);
}

async function saveEditedUrl() {
    editUrlForm.classList.add("was-validated");
    if (!editUrlForm.checkValidity()) return;

    const shortCode = editShortCodeInput.value;
    const originalUrl = editOriginalUrlInput.value.trim();

    let expiresAt = null;
    if (editExpiryDateInput.value && editExpiryDateInput.value.trim() !== "") {
        const expiryDate = new Date(editExpiryDateInput.value);
        if (expiryDate <= Date.now()) {
            showToast("Дата истечения должна быть в будущем");
            return;
        }
        expiresAt = removeMilliseconds(expiryDate);
    }

    const currentExpiryValue = editExpiryDateInput.value.trim();
    if (originalUrl === editOriginalValues.originalUrl && currentExpiryValue === editOriginalValues.expiresAt) {
        editUrlModal.hide();
        showToast("Ссылка обновлена");
        return;
    }

    try {
        const response = await fetch(apiServerAddress + `/links/${shortCode}`, {
            method: "PUT",
            headers: {
                "Authorization": `Bearer ${getAuthToken()}`,
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ url: originalUrl, expiresAt })
        });

        if (!response.ok) {
            console.error(response);
            showToast("Не удалось обновить ссылку");
            return;
        }

        const data = await response.json();
        const tr = document.getElementById(shortCode);
        if (tr) {
            updateUrlRow(tr, data.url, data.expiresAt);
        }
        updateUrlCaches(shortCode, data.url, data.expiresAt);
        editUrlModal.hide();
        showToast("Ссылка обновлена");
    }
    catch (error) {
        console.error(error);
        showToast("Не удалось обновить ссылку");
    }
}

function createTableRow(url, shortCode, createdAt, expiresAt) {
    const tr = document.createElement("tr");
    tr.id = shortCode;
    tr.dataset.expiresAt = expiresAt || "";

    const shortUrl = buildShortUrl(shortCode);

    tr.innerHTML = `<td class="text-truncate" style="max-width: 260px;">
            <a href="${url}" class="link-body-emphasis text-decoration-none" target="_blank" rel="noopener noreferrer">
                ${url}
            </a>
        </td>
        <td>
            <a href="${shortUrl}" class="link-primary text-decoration-none" target="_blank" rel="noopener noreferrer">
                ${shortCode}
            </a>
        </td>
        <td>${formatDate(createdAt)}</td>
        <td>${expiresAt ? formatDate(expiresAt) : "Нет срока жизни"}</td>
        <td>
            <div class="d-flex justify-content-end gap-1">
                <button type="button" class="btn btn-sm btn-outline-primary edit-btn" title="Изменить"><i class="bi bi-pencil-square"></i></button>
                <button type="button" class="btn btn-sm btn-outline-secondary copy-btn" title="Копировать"><i class="bi bi-clipboard"></i></button>
                <button type="button" class="btn btn-sm btn-outline-danger delete-btn" title="Удалить"><i class="bi bi-trash3"></i></button>
            </div>
        </td>`;

    tr.querySelector(".edit-btn").addEventListener("click", () => {
        const currentUrl = tr.cells[0].querySelector("a").textContent.trim();
        const currentExpiresAt = tr.dataset.expiresAt || null;
        openEditModal(shortCode, currentUrl, currentExpiresAt);
    });
    tr.querySelector(".copy-btn").addEventListener("click", () => copyUrl(shortUrl));
    tr.querySelector(".delete-btn").addEventListener("click", () => deleteUrl(shortCode));

    return tr;
}

function showUrls(urlsList) {
    urlsContainer.urlsTableBody.innerHTML = "";
    if (urlsList.length === 0) {
        showUrlsContainerElement(urlsContainer.emptyIndicator);
        return;
    }

    for (const url of [...urlsList].reverse()) {
        const row = createTableRow(url.url, url.shortCode, url.createdAt, url.expiresAt);
        urlsContainer.urlsTableBody.append(row);
    }

    showUrlsContainerElement(urlsContainer.urlsTable);
}

async function fetchUserLinks(excludeExpired) {
    const url = excludeExpired
        ? apiServerAddress + "/links"
        : apiServerAddress + "/links?ExcludeExpiredUrls=false";
    const response = await fetch(url, {
        method: "GET",
        headers: {
            "Authorization": `Bearer ${getAuthToken()}`,
            "Content-Type": "application/json"
        }
    });
    return response;
}

async function loadUserUrls() {//TODO: добавить пагинацию
    try {
        const response = await fetchUserLinks(true);
        if (!response.ok) {
            console.warn(response);
            showUrlsContainerElement(urlsContainer.errorIndicator);
            return;
        }

        const urls = await response.json();
        urlsCacheActive = urls;
        urlsShowExpiredCheckbox.disabled = false;
        if (urlsShowExpiredCheckbox.checked) {
            await ensureUrlsAllLoaded();
        } else {
            showUrls(urls);
        }
    }
    catch (error) {
        console.error(error);
        showUrlsContainerElement(urlsContainer.errorIndicator);
    }
}

async function ensureUrlsAllLoaded() {
    if (urlsCacheAll) {
        showUrls(urlsCacheAll);
        return;
    }
    try {
        showUrlsContainerElement(urlsContainer.loadingIndicator);
        const response = await fetchUserLinks(false);
        if (!response.ok) {
            console.warn(response);
            showUrlsContainerElement(urlsContainer.errorIndicator);
            return;
        }
        urlsCacheAll = await response.json();
        showUrls(urlsCacheAll);
    }
    catch (error) {
        console.error(error);
        showUrlsContainerElement(urlsContainer.errorIndicator);
    }
}

document.addEventListener("DOMContentLoaded", async () => {
    await loadUserUrls();

    urlsShowExpiredCheckbox.addEventListener("change", async () => {
        if (urlsShowExpiredCheckbox.checked) {
            await ensureUrlsAllLoaded();
        } else if (urlsCacheActive) {
            showUrls(urlsCacheActive);
        } else {
            await loadUserUrls();
        }
    });

    editUrlSaveBtn.addEventListener("click", saveEditedUrl);

    document.getElementById("edit-expiry-clear").addEventListener("click", () => {
        editExpiryDateInput.value = "";
    });
});
