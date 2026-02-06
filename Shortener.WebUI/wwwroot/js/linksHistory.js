import {apiServerAddress} from "./config.js";
import {formatDate, buildShortUrl} from "./common.js";

const linksLoading = document.getElementById("links-loading");
const linksEmpty = document.getElementById("links-empty");
const linksError = document.getElementById("links-error");
const linksTableWrap = document.getElementById("links-table-wrap");
const linksTbody = document.getElementById("links-tbody");
const linksMeta = document.getElementById("links-meta");

const toastEl = document.getElementById("app-toast");
const toastBody = document.getElementById("toast-body");
const toast = new bootstrap.Toast(toastEl, { delay: 2200 });

function showToast(message) {
    toastBody.textContent = message;
    toast.show();
}

function formatDate(value) {
    if (!value) return "—";
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return "—";
    return d.toLocaleString("ru-RU", { dateStyle: "medium", timeStyle: "short" });
}

function buildShortUrl(shortCode) {
    return window.location.origin + "/" + shortCode;
}

function setVisibility(loading, empty, error, table) {
    linksLoading.classList.toggle("d-none", !loading);
    linksEmpty.classList.toggle("d-none", !empty);
    linksError.classList.toggle("d-none", !error);
    linksTableWrap.classList.toggle("d-none", !table);
}

async function loadLinks() {
    setVisibility(true, false, false, false);
    linksError.textContent = "";

    try {
        const response = await fetch(`${apiServerAddress}/links?excludeExpiredUrls=false`);
        if (!response.ok) throw new Error("Не удалось загрузить список ссылок");
        const data = await response.json();
        renderLinks(data);
    } catch (err) {
        linksError.textContent = err.message || "Ошибка при загрузке списка ссылок.";
        setVisibility(false, false, true, false);
    }
}

function renderLinks(links) {
    if (!links || links.length === 0) {
        setVisibility(false, true, false, false);
        linksMeta.textContent = "";
        return;
    }

    setVisibility(false, false, false, true);
    linksMeta.textContent = `Всего: ${links.length}`;

    linksTbody.innerHTML = links
        .map(
            (item) => `
        <tr data-short-code="${escapeHtml(item.shortCode)}">
            <td>
                <a href="${escapeHtml(item.originalUrl)}" target="_blank" rel="noopener noreferrer" class="link-original text-truncate d-inline-block" style="max-width: 220px;" title="${escapeHtml(item.originalUrl)}">${escapeHtml(item.originalUrl)}</a>
            </td>
            <td>
                <a href="${escapeHtml(buildShortUrl(item.shortCode))}" target="_blank" rel="noopener noreferrer" class="link-short text-decoration-none">${escapeHtml(item.shortCode)}</a>
            </td>
            <td class="link-date">${formatDate(item.createdAt)}</td>
            <td class="link-date">${formatDate(item.expiresAt)}</td>
            <td class="text-end">
                <button type="button" class="btn btn-sm btn-outline-danger btn-delete" data-short-code="${escapeHtml(item.shortCode)}" aria-label="Удалить ссылку">Удалить</button>
            </td>
        </tr>
    `
        )
        .join("");

    linksTbody.querySelectorAll(".btn-delete").forEach((btn) => {
        btn.addEventListener("click", handleDelete);
    });
}

function escapeHtml(text) {
    if (text == null) return "";
    const div = document.createElement("div");
    div.textContent = text;
    return div.innerHTML;
}

async function handleDelete(e) {
    const btn = e.currentTarget;
    const shortCode = btn.getAttribute("data-short-code");
    if (!shortCode) return;

    btn.disabled = true;
    try {
        const response = await fetch(`${apiServerAddress}/links/${encodeURIComponent(shortCode)}`, {
            method: "DELETE"
        });
        if (response.status === 204) {
            const row = btn.closest("tr");
            if (row) row.remove();
            showToast("Ссылка удалена");
            if (linksTbody.querySelectorAll("tr").length === 0) {
                setVisibility(false, true, false, false);
                linksMeta.textContent = "";
            } else {
                linksMeta.textContent = `Всего: ${linksTbody.querySelectorAll("tr").length}`;
            }
        } else {
            showToast("Не удалось удалить ссылку");
        }
    } catch {
        showToast("Не удалось удалить ссылку");
    } finally {
        btn.disabled = false;
    }
}

loadLinks();
