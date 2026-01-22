// Template Manager for handling template selection, editing, and preview
class TemplateManager {
    constructor() {
        this.currentCategory = null;
        this.currentTemplate = null;
        this.templates = {};
        this.init();
    }

    init() {
        // Load templates for the first active tab
        const firstTab = document.querySelector('.nav-link.active');
        if (firstTab) {
            this.currentCategory = firstTab.getAttribute('data-category');
            this.loadTemplates(this.currentCategory);
        }

        // Attach event listeners to tabs
        document.querySelectorAll('[data-category]').forEach(tab => {
            tab.addEventListener('click', (e) => {
                const category = e.target.getAttribute('data-category');
                this.currentCategory = category;
                this.loadTemplates(category);
            });
        });

        // Save template button
        document.getElementById('saveTemplateBtn')?.addEventListener('click', () => {
            this.saveTemplate();
        });

        // Auto-update preview as user types
        document.getElementById('templateCode')?.addEventListener('input', (e) => {
            this.updatePreview(e.target.value);
        });
    }

    async loadTemplates(category) {
        const loadingEl = document.getElementById(`loading-${category}`);
        const gridEl = document.getElementById(`grid-${category}`);
        const emptyEl = document.getElementById(`empty-${category}`);

        // Show loading
        loadingEl.style.display = 'block';
        gridEl.style.display = 'none';
        emptyEl.style.display = 'none';

        try {
            const response = await fetch(`/api/templates/${category}`);
            const templates = await response.json();

            this.templates[category] = templates;

            // Hide loading
            loadingEl.style.display = 'none';

            if (templates.length === 0) {
                emptyEl.style.display = 'block';
            } else {
                gridEl.style.display = 'flex';
                this.renderTemplates(category, templates);
            }
        } catch (error) {
            console.error('Error loading templates:', error);
            loadingEl.style.display = 'none';
            emptyEl.style.display = 'block';
            this.showNotification('Error loading templates', 'danger');
        }
    }

    renderTemplates(category, templates) {
        const gridEl = document.getElementById(`grid-${category}`);
        const cardTemplate = document.getElementById('template-card-template');

        gridEl.innerHTML = '';

        templates.forEach(template => {
            const card = cardTemplate.content.cloneNode(true);

            // Load and display template preview
            const previewFrame = card.querySelector('.template-preview-frame');
            this.loadTemplatePreview(category, template.name, previewFrame);

            // Set title and date
            card.querySelector('.template-title').textContent = template.title;
            const date = new Date(template.modifiedAt);
            card.querySelector('.template-date').textContent = `Modified: ${date.toLocaleDateString()}`;

            // Preview button
            card.querySelector('.preview-btn').addEventListener('click', () => {
                this.previewTemplate(category, template.name);
            });

            // Edit button
            card.querySelector('.edit-btn').addEventListener('click', () => {
                this.editTemplate(category, template.name);
            });

            // Use button
            card.querySelector('.use-btn').addEventListener('click', () => {
                this.useTemplate(category, template.name);
            });

            // Delete button
            card.querySelector('.delete-btn').addEventListener('click', () => {
                this.deleteTemplate(category, template.name);
            });

            gridEl.appendChild(card);
        });
    }

    async loadTemplatePreview(category, templateName, previewElement) {
        try {
            const response = await fetch(`/api/templates/${category}/${templateName}`);
            const data = await response.json();
            
            // Create a scaled-down iframe preview
            const iframe = document.createElement('iframe');
            iframe.style.width = '100%';
            iframe.style.height = '100%';
            iframe.style.border = 'none';
            iframe.style.transform = 'scale(0.5)';
            iframe.style.transformOrigin = 'top left';
            iframe.style.width = '200%';
            iframe.style.height = '200%';
            iframe.style.pointerEvents = 'none';
            
            previewElement.appendChild(iframe);
            
            // Write content to iframe
            iframe.contentDocument.open();
            iframe.contentDocument.write(`
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body { margin: 0; padding: 20px; background: #f5f5f5; overflow: hidden; }
                    </style>
                </head>
                <body>${data.content}</body>
                </html>
            `);
            iframe.contentDocument.close();
        } catch (error) {
            console.error('Error loading template preview:', error);
            previewElement.innerHTML = '<div style="display: flex; align-items: center; justify-content: center; height: 100%; background: #f8f9fa; color: #666;"><i class="fas fa-file-code fa-3x"></i></div>';
        }
    }

    async editTemplate(category, templateName) {
        try {
            const response = await fetch(`/api/templates/${category}/${templateName}`);
            const data = await response.json();

            document.getElementById('currentCategory').value = category;
            document.getElementById('currentTemplateName').value = templateName;
            document.getElementById('templateCode').value = data.content;

            this.updatePreview(data.content);

            const modal = new bootstrap.Modal(document.getElementById('templateEditorModal'));
            modal.show();
        } catch (error) {
            console.error('Error loading template:', error);
            this.showNotification('Error loading template', 'danger');
        }
    }

    async previewTemplate(category, templateName) {
        try {
            const response = await fetch(`/api/templates/${category}/${templateName}`);
            const data = await response.json();

            document.getElementById('fullPreview').innerHTML = data.content;

            const modal = new bootstrap.Modal(document.getElementById('templatePreviewModal'));
            modal.show();
        } catch (error) {
            console.error('Error previewing template:', error);
            this.showNotification('Error previewing template', 'danger');
        }
    }

    useTemplate(category, templateName) {
        // Redirect to Designer with template parameters
        const fileName = templateName.endsWith('.html') ? templateName : `${templateName}.html`;
        window.location.href = `/Popup/Designer?templateFile=${encodeURIComponent(fileName)}&category=${encodeURIComponent(category)}`;
    }

    async saveTemplate() {
        const category = document.getElementById('currentCategory').value;
        const templateName = document.getElementById('currentTemplateName').value;
        const content = document.getElementById('templateCode').value;

        if (!content.trim()) {
            this.showNotification('Template content cannot be empty', 'warning');
            return;
        }

        const saveBtn = document.getElementById('saveTemplateBtn');
        const originalText = saveBtn.innerHTML;
        saveBtn.disabled = true;
        saveBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Saving...';

        try {
            const response = await fetch(`/api/templates/${category}/${templateName}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ content })
            });

            const result = await response.json();

            if (result.success) {
                this.showNotification('Template saved successfully', 'success');
                bootstrap.Modal.getInstance(document.getElementById('templateEditorModal')).hide();
                this.loadTemplates(category);
            } else {
                this.showNotification('Error saving template', 'danger');
            }
        } catch (error) {
            console.error('Error saving template:', error);
            this.showNotification('Error saving template', 'danger');
        } finally {
            saveBtn.disabled = false;
            saveBtn.innerHTML = originalText;
        }
    }

    async deleteTemplate(category, templateName) {
        if (!confirm(`Are you sure you want to delete "${templateName}"?`)) {
            return;
        }

        try {
            const response = await fetch(`/api/templates/${category}/${templateName}`, {
                method: 'DELETE'
            });

            const result = await response.json();

            if (result.success) {
                this.showNotification('Template deleted successfully', 'success');
                this.loadTemplates(category);
            } else {
                this.showNotification('Error deleting template', 'danger');
            }
        } catch (error) {
            console.error('Error deleting template:', error);
            this.showNotification('Error deleting template', 'danger');
        }
    }

    updatePreview(html) {
        const previewEl = document.getElementById('templatePreview');
        if (previewEl) {
            // Sanitize and render HTML
            previewEl.innerHTML = html;
        }
    }

    showNotification(message, type = 'info') {
        // Create Bootstrap alert
        const alertDiv = document.createElement('div');
        alertDiv.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
        alertDiv.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
        alertDiv.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        `;
        document.body.appendChild(alertDiv);

        // Auto-dismiss after 3 seconds
        setTimeout(() => {
            alertDiv.remove();
        }, 3000);
    }
}

// Create new template
function createNewTemplate(category) {
    const templateName = prompt('Enter template name (without .html):');
    if (!templateName) return;

    const fileName = templateName.endsWith('.html') ? templateName : `${templateName}.html`;
    
    document.getElementById('currentCategory').value = category;
    document.getElementById('currentTemplateName').value = fileName;
    document.getElementById('templateCode').value = `<!-- ${templateName} Template -->
<div class="template-container">
    <h2>Your Template Title</h2>
    <p>Start editing your template here...</p>
</div>

<style>
.template-container {
    padding: 20px;
    text-align: center;
}
</style>`;

    const modal = new bootstrap.Modal(document.getElementById('templateEditorModal'));
    modal.show();
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    window.templateManager = new TemplateManager();
});
