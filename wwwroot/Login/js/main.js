$(function () {
    'use strict';

    $('.form-control').on('input', function () {
        var $field = $(this).closest('.form-group');
        if (this.value) {
            $field.addClass('field--not-empty');
        } else {
            $field.removeClass('field--not-empty');
        }
    });

    $('#loginForm').on('submit', function () {
        let email = $('#UserName').val().trim();
        let password = $('#Password').val().trim();

        if (!email || !password) {
            alert('Vui lòng nhập đầy đủ thông tin');
            return false; // Ngăn form submit
        }
    });
});
