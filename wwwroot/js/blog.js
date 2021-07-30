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

const commentContainer = document.querySelector(".comment-container");

commentContainer.addEventListener('click', (e) => {
    if (e.target && e.target.className === "edit-comment") {
        let editBtn = e.target;
        editBtn.innerHTML = editBtn.innerHTML == "Edit" ? "Cancel" : "Edit";

        let id = editBtn.dataset.id;
        let comment = document.querySelector(`.comment[data-id="${id}"]`);
        console.log(comment);

        let commentContent = comment.querySelector(".comment-text");
        console.log(commentContent);
        commentContent.classList.toggle("hidden-element");


        let editForm = comment.querySelector(".edit-comment-form");
        console.log(editForm)
        editForm.classList.toggle("hidden-element");

    }
})