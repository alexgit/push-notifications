
$(function () {
    var connection = $.connection('/relay');

    connection.start(function () {
        console.log("connection started!");
    });

    connection.received(function (data) {
        console.log(data);
    });

    console.log(connection.id);
});