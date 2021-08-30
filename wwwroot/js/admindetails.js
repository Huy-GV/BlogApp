const banBtn = document.querySelector(".ban-btn");
const cancelBtn = document.querySelector(".cancel-btn");
const confirmBanBtn = document.querySelector(".confirm-ban-btn")
const form = document.querySelector(".suspend-form")

form.addEventListener("click", event => {
    if (event.target 
        && event.target.className === 'ban-btn' 
        || event.target.className === 'cancel-btn' ) {
            banBtn.classList.toggle('hidden-element')
            cancelBtn.classList.toggle('hidden-element')
            confirmBanBtn.classList.toggle('hidden-element')
        }
})
