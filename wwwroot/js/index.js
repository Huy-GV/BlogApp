const addBtn = document.querySelector(".add-button");
const addForm = document.querySelector(".add-form");

addBtn.addEventListener("click", () => {
    addBtn.innerHTML = addBtn.innerHTML === "Make a post !" ? "Cancel" : "Make a post !";
    addBtn.classList.toggle("add-button");
    addBtn.classList.toggle("cancel-post");
    addForm.classList.toggle("hidden-element");
})
