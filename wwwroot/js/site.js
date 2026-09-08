// ── Toast Notification Utility ──
function showToast(message, type = 'success') {
    let container = document.getElementById('toastContainer');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toastContainer';
        container.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        container.style.zIndex = '1080';
        document.body.appendChild(container);
    }

    const toastId = 'toast-' + Date.now();
    const bgClass = type === 'success' ? 'text-bg-success' : (type === 'danger' ? 'text-bg-danger' : 'text-bg-primary');
    const icon = type === 'success' ? 'bi-check-circle-fill' : (type === 'danger' ? 'bi-exclamation-triangle-fill' : 'bi-info-circle-fill');

    const toastHtml = `
        <div id="${toastId}" class="toast align-items-center ${bgClass} border-0 shadow-elevated" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body d-flex align-items-center gap-2">
                    <i class="bi ${icon} fs-6"></i>
                    <span>${message}</span>
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;

    container.insertAdjacentHTML('beforeend', toastHtml);
    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement, { delay: 4000 });
    toast.show();

    toastElement.addEventListener('hidden.bs.toast', () => {
        toastElement.remove();
    });
}

// ── Like Toggle Handler (AJAX) ──
$(document).ready(function () {
    $('#btnLike').on('click', function (e) {
        e.preventDefault();
        const $btn = $(this);
        const articleId = $btn.data('article-id');
        const token = $('input[name="__RequestVerificationToken"]').first().val();

        $btn.prop('disabled', true);

        $.ajax({
            url: '/Likes/Toggle',
            type: 'POST',
            data: {
                articleId: articleId,
                __RequestVerificationToken: token
            },
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: function (response) {
                if (response.success) {
                    // Update button appearance
                    if (response.isLiked) {
                        $btn.removeClass('btn-outline-danger').addClass('btn-danger');
                        $btn.find('i').removeClass('bi-heart').addClass('bi-heart-fill');
                        $('#btnLikeText').text('Disukai');
                    } else {
                        $btn.removeClass('btn-danger').addClass('btn-outline-danger');
                        $btn.find('i').removeClass('bi-heart-fill').addClass('bi-heart');
                        $('#btnLikeText').text('Suka Artikel');
                    }

                    // Update all like counter elements
                    $('.like-counter').text(response.totalLikes);

                    showToast(response.message, response.isLiked ? 'success' : 'info');
                } else {
                    showToast(response.message || 'Terjadi kesalahan.', 'danger');
                }
            },
            error: function (xhr) {
                if (xhr.status === 401) {
                    showToast('Silakan masuk terlebih dahulu untuk menyukai artikel.', 'danger');
                    setTimeout(() => {
                        window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
                    }, 1200);
                } else {
                    showToast('Gagal memproses suka. Coba beberapa saat lagi.', 'danger');
                }
            },
            complete: function () {
                $btn.prop('disabled', false);
            }
        });
    });

    // ── Comment Submission Handler (AJAX) ──
    $('#commentForm').on('submit', function (e) {
        e.preventDefault();
        const $form = $(this);
        const $textarea = $('#commentContent');
        const content = $textarea.val().trim();

        if (!content) {
            showToast('Silakan tulis isi komentar Anda.', 'danger');
            $textarea.focus();
            return;
        }

        const $submitBtn = $form.find('button[type="submit"]');
        $submitBtn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-1" role="status"></span> Mengirim...');

        $.ajax({
            url: $form.attr('action'),
            type: 'POST',
            data: $form.serialize(),
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: function (response) {
                if (response.success && response.comment) {
                    const c = response.comment;
                    const initial = (c.userDisplayName || 'U').charAt(0).toUpperCase();

                    const newCommentHtml = `
                        <div class="card border shadow-subtle mb-3 animate-fade-in" id="comment-${c.id}">
                            <div class="card-body p-3">
                                <div class="d-flex justify-content-between align-items-start mb-2">
                                    <div class="d-flex align-items-center gap-2">
                                        <span class="avatar-circle-sm" style="width: 30px; height: 30px; font-size: 0.8rem;">
                                            ${initial}
                                        </span>
                                        <div>
                                            <div class="fw-bold small text-dark">${c.userDisplayName}</div>
                                            <div class="text-muted" style="font-size: 0.72rem;">${c.createdAt}</div>
                                        </div>
                                    </div>
                                    <form action="/Comments/Delete/${c.id}" method="post" class="d-inline delete-comment-form">
                                        <input type="hidden" name="__RequestVerificationToken" value="${$form.find('input[name="__RequestVerificationToken"]').val()}" />
                                        <button type="submit" class="btn btn-link text-danger p-0 border-0 btn-delete-comment" title="Hapus Komentar" style="font-size: 0.85rem;">
                                            <i class="bi bi-trash"></i>
                                        </button>
                                    </form>
                                </div>
                                <p class="mb-0 text-secondary small" style="white-space: pre-line;">${c.content}</p>
                            </div>
                        </div>
                    `;

                    $('#noCommentsMessage').remove();
                    $('#commentsList').prepend(newCommentHtml);
                    $('#commentTotalBadge').text(response.totalComments);
                    $textarea.val('');
                    showToast(response.message, 'success');
                } else {
                    showToast(response.message || 'Gagal mengirim komentar.', 'danger');
                }
            },
            error: function (xhr) {
                if (xhr.status === 401) {
                    showToast('Silakan masuk terlebih dahulu.', 'danger');
                } else {
                    const err = xhr.responseJSON ? xhr.responseJSON.message : 'Terjadi kesalahan saat mengirim komentar.';
                    showToast(err, 'danger');
                }
            },
            complete: function () {
                $submitBtn.prop('disabled', false).html('<i class="bi bi-send me-1"></i> Kirim Komentar');
            }
        });
    });

    // ── Comment Delete Handler (AJAX Delegation) ──
    $(document).on('submit', '.delete-comment-form', function (e) {
        e.preventDefault();
        if (!confirm('Apakah Anda yakin ingin menghapus komentar ini?')) {
            return;
        }

        const $form = $(this);
        const url = $form.attr('action');
        const token = $form.find('input[name="__RequestVerificationToken"]').val();
        const $card = $form.closest('.card');

        $.ajax({
            url: url,
            type: 'POST',
            data: {
                __RequestVerificationToken: token
            },
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: function (response) {
                if (response.success) {
                    $card.fadeOut(300, function () {
                        $(this).remove();
                        if (response.totalComments !== undefined) {
                            $('#commentTotalBadge').text(response.totalComments);
                            if (response.totalComments === 0) {
                                $('#commentsList').html(`
                                    <div class="p-4 text-center text-muted bg-light rounded-3 border small" id="noCommentsMessage">
                                        Belum ada komentar pada artikel ini. Jadilah yang pertama berkomentar!
                                    </div>
                                `);
                            }
                        }
                    });
                    showToast(response.message, 'info');
                } else {
                    showToast(response.message || 'Gagal menghapus komentar.', 'danger');
                }
            },
            error: function () {
                showToast('Gagal menghapus komentar. Coba lagi.', 'danger');
            }
        });
    });
});

// ── Expandable Navbar Search ──
(function () {
    document.addEventListener('DOMContentLoaded', function () {
        const wrap = document.querySelector('.navbar-search-wrap');
        const trigger = document.getElementById('navSearchTrigger');
        const closeBtn = document.getElementById('navSearchClose');
        const input = document.getElementById('navSearchInput');
        const form = document.getElementById('navSearchForm');

        if (!wrap || !trigger || !closeBtn || !input) return;

        function openSearch() {
            wrap.classList.add('is-open');
            setTimeout(() => input.focus(), 50);
        }

        function closeSearch() {
            wrap.classList.remove('is-open');
            input.value = '';
        }

        trigger.addEventListener('click', openSearch);
        closeBtn.addEventListener('click', closeSearch);

        // Close on Escape key
        input.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') closeSearch();
        });

        // Close when clicking outside on desktop
        document.addEventListener('click', function (e) {
            if (!wrap.contains(e.target) && wrap.classList.contains('is-open')) {
                closeSearch();
            }
        });

        // Prevent form submit if empty
        form.addEventListener('submit', function (e) {
            if (!input.value.trim()) {
                e.preventDefault();
                input.focus();
            }
        });
    });
})();
