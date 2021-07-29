const editForm = document.querySelector(".edit-blog-form");
const editBlogBtn = document.querySelector(".edit-post");
const blogContent = document.querySelector(".blog-content");

editBlogBtn.addEventListener('click', () => {
    editBlogBtn.innerHTML = editBlogBtn.innerHTML == "Edit" ? "Cancel" : "Edit";
    editForm.classList.toggle("hidden-element");
    blogContent.classList.toggle("hidden-element");
})

const addBtn = document.querySelector(".add-button");
const addForm = document.querySelector(".add-form");

addBtn.addEventListener("click", () => {
    addBtn.innerHTML = addBtn.innerHTML == "Make a post !" ? "Cancel" : "Make a post !";
    addForm.classList.toggle("hidden-element");
})

const commentForm = document.querySelector(".comment-form");
const commentBtn = document.querySelector(".comment-button");

commentBtn.addEventListener("click", () => {
    commentBtn.classList.toggle("hidden-element");
    commentForm.classList.toggle("hidden-element");
})

const cancelCommentBtn = document.querySelector(".cancel-comment");

cancelCommentBtn.addEventListener("click", () => {
    commentBtn.classList.toggle("hidden-element");
    commentForm.classList.toggle("hidden-element");
})

const editCommentBtn = document.querySelector(".edit-comment");

editCommentBtn.addEventListener('click', () => {
    
})