const commentForm = document.querySelector(".comment-form");
const commentBtn = document.querySelector(".comment-button");
const cancelCommentBtn = document.querySelector(".cancel-comment");

commentBtn.addEventListener("click", () => {
    commentBtn.classList.toggle("hidden-element");
    commentForm.classList.toggle("hidden-element");
})

cancelCommentBtn.addEventListener("click", () => {
    commentBtn.classList.toggle("hidden-element");
    commentForm.classList.toggle("hidden-element");
})