import {apiServerAddress} from "./config.js";
import {getAuthToken} from "./common.js";

const urlsLoadingIndicator = document.getElementById("links-loading"); // Индикатор загрузки списка ссылок
const urlsLoadingErrorIndicator = document.getElementById("links-error"); // Индикатор ошибки при неудачной загрузки ссылок
const urlsContainer = document.getElementById("links-table-wrap"); // Контейнер со списком ссылок при их удачной загрузки и не пустом списке ссылок
const urlsEmptyContainer = document.getElementById("links-empty"); // Индикатор пустого списка ссылок

const linksList = document.getElementById("links-tbody"); // Сам список ссылок

const urlsContainerElements = document.getElementById("urlsContainer").children; // Индикаторы отображения состояния списка ссылок

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

function createTableRow(originalUrl, shortCode, createdAt, expiresAt) {
    const tr = document.createElement("tr");

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

document.addEventListener("DOMContentLoaded", async () => {
    showUrlsContainerElement(urlsLoadingIndicator);
    await loadUserUrls();
});