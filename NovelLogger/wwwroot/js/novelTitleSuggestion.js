const input = document.getElementById("novelTitle");
const list = document.getElementById("suggestions");
let lastController = null;

input.addEventListener("input", async () => {
    const text = input.value;
    list.innerHTML = "";

    if (text.length < 2) {
        return;
    }
    
    if (lastController) {
        lastController.abort();
    }
    lastController = new AbortController();

    const result = await fetch(`/Bookmark/NovelTitleSuggestions?title=${encodeURIComponent(text)}`,
        { signal: lastController.signal }).catch(() => null);

    if (!result || !result.ok){
        return;
    }

    const items = await result.json();

    for (const item of items) {
        const li = document.createElement("li");
        li.className = "list-group-item list-group-item-action";
        li.textContent = item;

        li.addEventListener("click", () => {
            input.value = item;
            list.innerHTML = "";
        })
        list.append(li);
    }
});