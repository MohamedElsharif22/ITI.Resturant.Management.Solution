(function() {
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    let editModal;

    document.addEventListener('DOMContentLoaded', function() {
        editModal = new bootstrap.Modal(document.getElementById('menuEditModal'));

        document.querySelectorAll('.toggle-btn').forEach(btn => btn.addEventListener('click', onToggle));
        document.querySelectorAll('.delete-btn').forEach(btn => btn.addEventListener('click', onDelete));
        document.querySelectorAll('.edit-btn').forEach(btn => btn.addEventListener('click', onEdit));

        document.getElementById('saveMenuBtn')?.addEventListener('click', onSave);

        // preview image
        const imgInput = document.getElementById('menuImage');
        if (imgInput) imgInput.addEventListener('change', onImagePreview);
    });

    async function onToggle(e) {
        const btn = e.currentTarget;
        const id = btn.dataset.id;
        if (!id) return;

        btn.disabled = true;
        try {
            const resp = await fetch(`/Admin/Menu/ToggleAvailability`, {
                method: 'POST',
                headers: new Headers({
                    'RequestVerificationToken': token,
                    'Content-Type': 'application/json'
                }),
                body: JSON.stringify({ id: parseInt(id) })
            });

            if (resp.ok) {
                const data = await resp.json();
                const row = document.querySelector(`tr[data-id="${id}"]`);
                const availCell = row.querySelector('.item-available');
                const toggleBtn = row.querySelector('.toggle-btn');
                if (data.isAvailable) {
                    availCell.innerHTML = '<span class="badge bg-success">Yes</span>';
                    toggleBtn.textContent = 'Disable';
                    toggleBtn.classList.remove('btn-outline-success');
                    toggleBtn.classList.add('btn-outline-warning');
                } else {
                    availCell.innerHTML = '<span class="badge bg-secondary">No</span>';
                    toggleBtn.textContent = 'Enable';
                    toggleBtn.classList.remove('btn-outline-warning');
                    toggleBtn.classList.add('btn-outline-success');
                }
                toggleBtn.dataset.available = data.isAvailable;
            } else {
                alert('Failed to toggle availability.');
            }
        } catch (err) {
            console.error(err);
            alert('An error occurred while toggling availability.');
        } finally {
            btn.disabled = false;
        }
    }

    async function onDelete(e) {
        const btn = e.currentTarget;
        const id = btn.dataset.id;
        if (!id) return;

        if (!confirm('Delete this item?')) return;
        btn.disabled = true;
        try {
            const formData = new FormData();
            formData.append('id', id);

            const resp = await fetch(`/Admin/Menu/Delete`, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': token
                },
                body: formData
            });

            if (resp.ok) {
                const row = document.querySelector(`tr[data-id="${id}"]`);
                if (row) row.remove();
            } else {
                alert('Failed to delete item.');
            }
        } catch (err) {
            console.error(err);
            alert('An error occurred while deleting item.');
        } finally {
            btn.disabled = false;
        }
    }

    async function onEdit(e) {
        const id = e.currentTarget.dataset.id;
        if (!id) return;
        try {
            const resp = await fetch(`/Admin/Menu/GetMenuItem?id=${id}`);
            if (!resp.ok) return alert('Failed to load item');
            const item = await resp.json();

            // populate form
            document.getElementById('menuItemId').value = item.id;
            document.getElementById('menuName').value = item.name || '';
            document.getElementById('menuDescription').value = item.description || '';
            document.getElementById('menuPrice').value = item.price || '';
            document.getElementById('menuCategory').value = item.categoryId || '';
            document.getElementById('menuPrep').value = item.preparationTimeInMinutes || '';
            document.getElementById('menuAvailable').checked = item.isAvailable;

            const imgPreview = document.getElementById('menuImagePreview');
            imgPreview.innerHTML = item.imageUrl ? `<img src="${item.imageUrl}" class="img-fluid" style="max-height:120px;" />` : '';

            editModal.show();
        } catch (err) {
            console.error(err);
            alert('An error occurred while loading item.');
        }
    }

    function onImagePreview(e) {
        const file = e.target.files[0];
        const preview = document.getElementById('menuImagePreview');
        if (!file) { preview.innerHTML = ''; return; }
        const reader = new FileReader();
        reader.onload = function(ev) {
            preview.innerHTML = `<img src="${ev.target.result}" class="img-fluid" style="max-height:120px;" />`;
        };
        reader.readAsDataURL(file);
    }

    async function onSave() {
        const form = document.getElementById('menuEditForm');
        const fd = new FormData(form);
        const id = document.getElementById('menuItemId').value;
        if (id) fd.append('Id', id);

        try {
            const resp = await fetch(`/Admin/Menu/Save`, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': token
                },
                body: fd
            });

            if (resp.ok) {
                const data = await resp.json();
                // Update row in table if exists
                const row = document.querySelector(`tr[data-id="${data.id}"]`);
                if (row) {
                    // replace row contents by reloading the page row or updating fields
                    // For simplicity, update name, price, prep, available and image
                    const item = {
                        name: fd.get('Name'),
                        price: parseFloat(fd.get('Price')),
                        preparationTimeInMinutes: fd.get('PreparationTimeInMinutes'),
                        categoryId: fd.get('CategoryId'),
                        isAvailable: fd.get('IsAvailable') === 'on'
                    };

                    row.querySelector('.item-name').textContent = item.name;
                    row.querySelector('.item-price').textContent = item.price.toLocaleString(undefined, { style: 'currency', currency: 'USD' });
                    row.querySelector('.item-prep').textContent = item.preparationTimeInMinutes;
                    row.querySelector('.item-available').innerHTML = item.isAvailable ? '<span class="badge bg-success">Yes</span>' : '<span class="badge bg-secondary">No</span>';

                    // image preview if new file provided
                    const fileInput = document.getElementById('menuImage');
                    if (fileInput && fileInput.files && fileInput.files.length) {
                        // reload row image from response if server returns image url; currently server returns only id
                        // simpler: reload the page to reflect image changes
                        location.reload();
                        return;
                    }
                } else {
                    // if new item added, reload to show it
                    location.reload();
                    return;
                }

                editModal.hide();
            } else {
                const txt = await resp.text();
                alert('Failed to save item: ' + txt);
            }
        } catch (err) {
            console.error(err);
            alert('An error occurred while saving item.');
        }
    }

})();
