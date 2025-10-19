$(document).ready(function () {
    // Sidebar toggle functionality
    $('#sidebarCollapse').on('click', function () {
        $('#sidebar').toggleClass('active');
        $('#content').toggleClass('active');

        // Update button text
        var span = $(this).find('span');
        if ($('#sidebar').hasClass('active')) {
            span.text('Show Sidebar');
        } else {
            span.text('Toggle Sidebar');
        }
    });

    // Auto-hide sidebar on mobile
    $(window).resize(function () {
        if ($(window).width() <= 768) {
            $('#sidebar').addClass('active');
            $('#content').addClass('active');
        } else {
            $('#sidebar').removeClass('active');
            $('#content').removeClass('active');
        }
    });

    // Initialize DataTables for admin tables (only if not already initialized)
    if ($.fn.DataTable) {
        $('.admin-table').each(function () {
            if (!$.fn.DataTable.isDataTable(this)) {
                $(this).DataTable({
                    "responsive": true,
                    "lengthChange": true,
                    "autoWidth": true,
                    "scrollX": true,
                    "pageLength": 25,
                    "order": [[0, "asc"]],
                    "language": {
                        "url": "//cdn.datatables.net/plug-ins/1.10.24/i18n/Vietnamese.json"
                    },
                    "columnDefs": [
                        { "orderable": false, "targets": -1 } // Last column (actions) not sortable
                    ]
                });
            }
        });
    }

    // Auto-dismiss alerts after 5 seconds
    setTimeout(function () {
        $('.alert').fadeOut('slow');
    }, 5000);

    // Confirm dialogs for dangerous actions
    $('.btn-danger, .btn-warning').on('click', function (e) {
        var action = $(this).attr('title') || 'thực hiện thao tác này';
        if (!confirm('Bạn có chắc chắn muốn ' + action + '?')) {
            e.preventDefault();
        }
    });

    // Loading state for forms
    $('form').on('submit', function () {
        var submitBtn = $(this).find('button[type="submit"]');
        if (submitBtn.length) {
            var originalText = submitBtn.html();
            submitBtn.html('<span class="loading"></span> Đang xử lý...');
            submitBtn.prop('disabled', true);

            // Re-enable after 10 seconds (fallback)
            setTimeout(function () {
                submitBtn.html(originalText);
                submitBtn.prop('disabled', false);
            }, 10000);
        }
    });

    // Search functionality
    $('.search-form').on('submit', function (e) {
        var searchTerm = $(this).find('input[name="search"]').val().trim();
        if (searchTerm === '') {
            e.preventDefault();
            return false;
        }
    });

    // Tooltip initialization
    $('[data-bs-toggle="tooltip"]').tooltip();

    // Dropdown menu handling
    $('.dropdown-toggle').on('click', function (e) {
        e.preventDefault();
        var target = $(this).attr('data-bs-target');
        $(target).collapse('toggle');
    });

    // Sidebar active state management
    var currentAction = window.location.pathname.split('/').pop();
    $('.sidebar a').each(function () {
        var href = $(this).attr('href');
        if (href && href.includes(currentAction)) {
            $(this).parent().addClass('active');
        }
    });

    // Smooth scrolling for anchor links
    $('a[href^="#"]').on('click', function (event) {
        var target = $(this.getAttribute('href'));
        if (target.length) {
            event.preventDefault();
            $('html, body').stop().animate({
                scrollTop: target.offset().top - 100
            }, 1000);
        }
    });

    // Form validation enhancement
    $('form').on('submit', function (e) {
        var form = $(this);
        var isValid = true;

        // Check required fields
        form.find('[required]').each(function () {
            if ($(this).val().trim() === '') {
                $(this).addClass('is-invalid');
                isValid = false;
            } else {
                $(this).removeClass('is-invalid');
            }
        });

        if (!isValid) {
            e.preventDefault();
            $('html, body').animate({
                scrollTop: form.find('.is-invalid').first().offset().top - 100
            }, 500);
        }
    });

    // Real-time character counter
    $('textarea[maxlength]').on('input', function () {
        var maxLength = $(this).attr('maxlength');
        var currentLength = $(this).val().length;
        var counter = $(this).siblings('.char-counter');

        if (counter.length === 0) {
            counter = $('<small class="char-counter text-muted"></small>');
            $(this).after(counter);
        }

        counter.text(currentLength + '/' + maxLength);

        if (currentLength > maxLength * 0.9) {
            counter.addClass('text-warning');
        } else {
            counter.removeClass('text-warning');
        }
    });

    // Initialize all tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});
