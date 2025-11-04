(function() {
    var roleModal = null;

    document.addEventListener('DOMContentLoaded', function() {
        roleModal = new bootstrap.Modal(document.getElementById('roleEditorModal'));
    });

    window.showRoleEditor = function(userId) {
        var userRoles = Array.from(document.querySelectorAll('#roles-' + userId + ' .badge'))
            .map(function(span) { return span.textContent.trim(); });
        
        document.querySelectorAll('.role-checkbox').forEach(function(cb) { 
            cb.checked = false; 
        });
        
        userRoles.forEach(function(role) {
            var cb = document.getElementById('role-' + role);
            if (cb) cb.checked = true;
        });
        
        document.getElementById('editingUserId').value = userId;
        roleModal.show();
    };

    window.saveRoles = async function() {
        var userId = document.getElementById('editingUserId').value;
        var selectedRoles = Array.from(document.querySelectorAll('.role-checkbox:checked'))
            .map(function(cb) { return cb.value; });
        
        try {
            var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
            var token = tokenInput ? tokenInput.value : '';

            // POST to the Users controller UpdateUserRoles action
            var response = await fetch('/Admin/Users/UpdateUserRoles?id=' + encodeURIComponent(userId), {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify(selectedRoles)
            });
            
            if (response.ok) {
                var rolesDisplay = document.getElementById('roles-' + userId);
                var html = selectedRoles
                    .map(function(r) { return '<span class="badge bg-primary me-1">' + r + '</span>'; })
                    .join(' ');
                rolesDisplay.innerHTML = html;
                roleModal.hide();
            } else {
                alert('Failed to update roles. Please try again.');
            }
        } catch (err) {
            console.error(err);
            alert('An error occurred while updating roles.');
        }
    };

    window.toggleLock = async function(userId, lock) {
        try {
            var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
            var token = tokenInput ? tokenInput.value : '';

            // POST to the Users controller Lock/Unlock actions
            var url = lock ? ('/Admin/Users/LockUser?id=' + encodeURIComponent(userId)) : ('/Admin/Users/UnlockUser?id=' + encodeURIComponent(userId));
            var response = await fetch(url, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': token
                }
            });
            
            if (response.ok) {
                location.reload();
            } else {
                alert('Failed to update user status. Please try again.');
            }
        } catch (err) {
            console.error(err);
            alert('An error occurred while updating user status.');
        }
    };
})();