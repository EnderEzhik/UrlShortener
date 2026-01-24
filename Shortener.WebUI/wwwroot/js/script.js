const urlsList = document.getElementById("url-history-list");

async function deleteUrl(shortCode) {
    try {
        const response = await fetch(`/api/links/${shortCode}`, { method: "DELETE" });
        if (!response.ok) {
            console.log("Error creating shortCode:\n" + "Status Code: " + response.status + "\n" + "Error: " + await response.text());
            return;
        }
        
        document.getElementById(shortCode).remove();
    }
    catch(error){
        console.log(error);
    }
}

function addUrlToUrlList(urlData) {
    const newUrlElement = document.createElement("li");
    newUrlElement.setAttribute("id", urlData.shortCode);
    newUrlElement.classList.add("url-item");
    newUrlElement.innerHTML = `<div class="url-content">
                        <div class="url-header">
                            <a href="redirect/${urlData.shortCode}" class="url-link">${urlData.shortCode}</a>
                        </div>
                        <span class="url-original">${urlData.originalUrl}</span>
                    </div>
                    <button class="delete-btn" aria-label="Удалить ссылку">×</button>`;
    
    if (urlData.expiresAt) {
        const expiredAtSpan = document.createElement("span");
        expiredAtSpan.classList.add("url-original");
        expiredAtSpan.textContent = new Date(urlData.expiresAt).toLocaleString();
        
        newUrlElement.firstChild.appendChild(expiredAtSpan);
    }
    
    const deleteBtn = newUrlElement.querySelector(".delete-btn");
    deleteBtn.addEventListener("click", async () => {
        await deleteUrl(urlData.shortCode);
    });
    
    urlsList.prepend(newUrlElement);
}

async function CreateShortUrl() {
    event.preventDefault();

    const url = document.getElementById("url-input").value;
    const urlExpires = document.getElementById("expiry-date").value;

    const requestBodyData = {
        url: url
    }

    if (urlExpires.trim() && urlExpires.trim().length > 0) {
        requestBodyData.expiresAt = new Date(urlExpires).toISOString();
    }

    try {
        const response = await fetch("/api/links", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(requestBodyData)
        });

        if (!response.ok) {
            console.log("Error creating shortCode:\n" + "Status Code: " + response.status + "\n" + "Error: " + await response.text());
            //TODO: добавить сообщение об ошибке
            return;
        }

        const data = await response.json();

        addUrlToUrlList(data);
        document.getElementById("short-url-form").reset();
    }
    catch (error) {
        console.log(error);
    }
}

function initListeners() {
    document.getElementById("short-url-form").addEventListener("submit", CreateShortUrl);
}

async function loadUrls() {
    try {
        const response = await fetch("/api/links");
        if (!response.ok) {
            console.log("Error getting urls:\n" + "Status Code: " + response.status + "\n" + "Error: " + await response.text());
            //TODO: добавить сообщение об ошибке загрузке данных
            return;
        }

        const urls = await response.json();
        urls.forEach((urlData) => {
            addUrlToUrlList(urlData);
        });
    }
    catch (error) {
        console.log(error);
    }
}

document.addEventListener("DOMContentLoaded", () => {
    initListeners();
    loadUrls();
})