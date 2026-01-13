// const urlsList = document.getElementsByTagName("url-history-list");
const urlsList = document.getElementById("url-history-list");

function addUrlToUrlList(urlData) {
    const newUrlElement = document.createElement("li");
    newUrlElement.setAttribute("id", urlData.shortCode);
    newUrlElement.classList.add("url-item");
    newUrlElement.innerHTML = `<div class="url-content">
                        <div class="url-header">
                            <a href="https://localhost:7000/${urlData.shortCode}" class="url-link">https://localhost:7000/${urlData.shortCode}</a>
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
    
    urlsList.prepend(newUrlElement);
}

function initForm() {
    document.getElementById("short-url-form").addEventListener("submit", async () => {
        event.preventDefault();
        
        const url = document.getElementById("url-input").value;
        const urlExpires = document.getElementById("expiry-date").value;
        
        const requestBodyData = {
            url: url
        }
        
        if (urlExpires.trim() && urlExpires.trim().length > 0) {
            const expiresDate = new Date(urlExpires).toISOString();
            requestBodyData.expiresAt = expiresDate;
        }
        
        console.log(requestBodyData);
        
        try {
            const response = await fetch("https://localhost:7000/api/links", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(requestBodyData)
            });

            if (!response.ok) {
                console.log("Error creating shortCode:\n" + "Status Code: " + response.status + "\n" + "Error: " + await response.text());
                return;
            }
            
            const data = await response.json();
            
            console.log(data);
            addUrlToUrlList(data);
        }
        catch (error) {
            console.log(error);
        }
    });
}

document.addEventListener("DOMContentLoaded", () => {
    initForm();
})