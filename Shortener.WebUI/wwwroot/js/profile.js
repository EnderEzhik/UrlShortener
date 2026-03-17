import {apiServerAddress} from "./config.js";
import {getAuthToken, buildShortUrl} from "./common.js";

const urlsLoadingIndicator = document.getElementById("links-loading"); // Индикатор загрузки списка ссылок
const urlsLoadingErrorIndicator = document.getElementById("links-error"); // Индикатор ошибки при неудачной загрузки ссылок
const urlsContainer = document.getElementById("links-table-wrap"); // Контейнер со списком ссылок при их удачной загрузки и не пустом списке ссылок
const urlsEmptyContainer = document.getElementById("links-empty"); // Индикатор пустого списка ссылок

const linksList = document.getElementById("links-tbody"); // Сам список ссылок

const urlsContainerElements = document.getElementById("urlsContainer").children; // Индикаторы отображения состояния списка ссылок

const profileContainer = {
    self: document.getElementById("profile-container"),
    loadingIndicator: document.getElementById("profile-loading"),
    errorIndicator: document.getElementById("profile-error"),
    content: document.getElementById("profile-content"),
    username: document.getElementById("profile-username"),
    registrationDate: document.getElementById("profile-registration-date")
};

function showUrlsContainerElement(element) {
    if (element.classList.contains("d-none")) {
        element.classList.remove("d-none");
    }

    for (const item of urlsContainerElements) {
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
            console.warn(response);
            alert("При удалении ссылки сервер вернул не 2** статус код");
            return;
        }
        deleteUrlFromUI(shortCode);
    }
    catch (error) {
        console.error(error);
        alert("Ошибка при удалении ссылки");
    }
}

async function copyUrl(text) {
    if (navigator.clipboard && window.isSecureContext) {
        await navigator.clipboard.writeText(text);
        return;
    }

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

function createTableRow(originalUrl, shortCode, createdAt, expiresAt) {
    const tr = document.createElement("tr");
    tr.id = shortCode;

    tr.innerHTML = `<td class="text-truncate" style="max-width: 260px;">
            <a href="${originalUrl}" class="link-body-emphasis text-decoration-none" target="_blank" rel="noopener noreferrer">
                ${originalUrl}
            </a>
        </td>
        <td>
            <a href="${shortCode}" class="link-primary text-decoration-none" target="_blank" rel="noopener noreferrer">
                ${shortCode}
            </a>
        </td>
        <td>${createdAt}</td>
        <td>${expiresAt ? expiresAt : "Нет срока жизни"}</td>
        <td>
            <div class="d-flex justify-content-end gap-2">
                <button type="button" id="copyBtn" class="btn btn-sm btn-outline-secondary">Копировать</button>
                <button type="button" id="deleteBtn" class="btn btn-sm btn-outline-danger">Удалить</button>
            </div>
        </td>`;
    
    tr.querySelector("[id=copyBtn]").addEventListener("click", () => copyUrl(buildShortUrl(shortCode)));
    tr.querySelector("[id=deleteBtn]").addEventListener("click", () => deleteUrl(shortCode));
    
    return tr;
}

function showUrls(urlsList) {
    if (urlsList.length === 0) {
        showUrlsContainerElement(urlsEmptyContainer);
        return;
    }
    
    urlsList.reverse();
    for (const url of urlsList) {
        const row = createTableRow(url.originalUrl, url.shortCode, url.createdAt, url.expiresAt);
        linksList.append(row);
    }

    showUrlsContainerElement(urlsContainer);
}

async function loadUserUrls() {
    try {
        const response = await fetch(apiServerAddress + "/links/me", {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${getAuthToken()}`,
                "Content-Type": "application/json"
            }
        });
        if (!response.ok) {
            console.warn(response);
            showUrlsContainerElement(urlsLoadingErrorIndicator);
            return;
        }
        
        const urls = await response.json();
        showUrls(urls);
    }
    catch (error) {
        console.error(error);
        showUrlsContainerElement(urlsLoadingErrorIndicator);
    }
}

async function loadUserProfile() {
    try {
        const response = await fetch(apiServerAddress + "/users/me", {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${getAuthToken()}`,
                "Content-Type": "application/json"
            }
        });
        if (!response.ok) {
            console.warn(response);
            profileContainer.loadingIndicator.classList.add("d-none");
            profileContainer.errorIndicator.classList.remove("d-none");
            return;
        }
        
        const userData = await response.json();
        const username = userData.username;
        const registrationDate = userData.registrationDate;
        
        profileContainer.username.textContent = username;
        profileContainer.registrationDate.textContent = new Date(registrationDate).toLocaleString("ru-RU", {dateStyle: "short"});
        profileContainer.loadingIndicator.classList.add("d-none");
        profileContainer.content.classList.remove("d-none");
    }
    catch (error) {
        profileContainer.loadingIndicator.classList.add("d-none");
        profileContainer.errorIndicator.classList.remove("d-none");
    }
}

document.addEventListener("DOMContentLoaded", () => {
    loadUserUrls();
    loadUserProfile();
});