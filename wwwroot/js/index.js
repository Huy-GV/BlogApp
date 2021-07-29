const addBtn = document.querySelector(".add-button");
const addForm = document.querySelector(".add-form");


addBtn.addEventListener("click", () => {
    addBtn.innerHTML = addBtn.innerHTML == "Make a post !" ? "Cancel" : "Make a post !";
    addForm.classList.toggle("hidden-element");
})

