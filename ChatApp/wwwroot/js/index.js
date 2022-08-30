var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

connection.start().then().catch(function (err) {
    return console.error(err.toString());
});

connection.on("ReloadPage", function (groupId) {
    window.location.replace(window.location.href.replace("Index", "Chat?groupId=" + groupId));
});

window.onbeforeunload = function () {
    $.ajax({
        type: 'POST',
        url: '/Respondent/RemoveRequest',
        cache: false
    });
}

var requestToContact = function (userId) {
    $.ajax({
        type: 'GET',
        url: '/Respondent/RequestContact',
        data: { userId: userId },
        cache: false,
        success: function () {
            alert("Send request successfully");
        },
        error: function (err) {
            alert(err.responseText);
        }
    });
}