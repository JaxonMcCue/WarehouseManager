$(document).ready(function () {
        var cleave = new Cleave('#phone', {
            delimiters: ["(", ") ", "-"],
            blocks: [0, 3, 3, 4],
            numericOnly: true
        });
    })