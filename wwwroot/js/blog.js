const blogContent = document.querySelector(".blog-content");
const blogContainer = document.querySelector(".blog-container");

blogContainer.addEventListener('click', (e) => {
    if (e.target && e.target.className === "edit-post") {
        let editBlogBtn = document.querySelector(".edit-post");
        let editForm = document.querySelector(".edit-blog-form");
        editBlogBtn.innerHTML = editBlogBtn.innerHTML == "Edit" ? "Cancel" : "Edit";
        editForm.classList.toggle("hidden-element");
        blogContent.classList.toggle("hidden-element");
    }

})

const commentContainer = document.querySelector(".comment-container");
const editBlogBtn = document.querySelector(".edit-blog");

commentContainer.addEventListener('click', (e) => {
    if (e.target && e.target.className === "edit-comment") {
        let editCommentBtn = e.target;
        editCommentBtn.innerHTML = editCommentBtn.innerHTML == "Edit" ? "Cancel" : "Edit";
        let id = editCommentBtn.dataset.id;
        let comment = document.querySelector(`.comment[data-id="${id}"]`);
        let commentContent = comment.querySelector(".comment-text");
        commentContent.classList.toggle("hidden-element");

        let editTextarea = comment.querySelector(".edit-comment-textarea");
        let saveCommentEditBtn = comment.querySelector(".save-btn")
        editTextarea.classList.toggle("hidden-element");
        saveCommentEditBtn.classList.toggle("hidden-element");

        let deleteCommentBtn = comment.querySelector(".delete-comment");
        deleteCommentBtn.classList.toggle("hidden-element");

    }
})
