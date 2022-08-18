function loadChat(name) {
    $(".chat-wrapper.shown").removeClass("shown");
    $("#select-chat-div").hide();

    $chat = $('*[data-recipient="' + name + '"]');
    $chat.addClass("shown");
    scrollToBottom();
}

$(".write input").keyup(function (event) {
    if (event.keyCode === 13) {
        $("#btn-send").click();
    }
});