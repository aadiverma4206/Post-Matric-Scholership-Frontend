// ==========================================================================
// Post Matric Scholarship Portal JavaScript
// National e-Governance Standards Implementation
// ==========================================================================

document.addEventListener('DOMContentLoaded', function () {
  initAccessibilityControls();
  initConfirmationMatchers();
  initCascadingDropdowns();
  initCaptchaRefresh();
  initLockModal();
  initFileUploadValidators();
  highlightActiveNavLinks();
});

// 1. Accessibility Controls (Font Sizing & High Contrast)
function initAccessibilityControls() {
  const currentScale = localStorage.getItem('gov_font_scale') || '1';
  document.documentElement.style.setProperty('--font-scale', currentScale + 'rem');

  const btnDecr = document.getElementById('btnFontDecr');
  const btnReset = document.getElementById('btnFontReset');
  const btnIncr = document.getElementById('btnFontIncr');
  const btnContrast = document.getElementById('btnContrastToggle');

  if (btnDecr) {
    btnDecr.addEventListener('click', function () {
      let scale = parseFloat(localStorage.getItem('gov_font_scale') || '1');
      if (scale > 0.85) {
        scale = Math.round((scale - 0.05) * 100) / 100;
        document.documentElement.style.setProperty('--font-scale', scale + 'rem');
        localStorage.setItem('gov_font_scale', scale.toString());
      }
    });
  }

  if (btnReset) {
    btnReset.addEventListener('click', function () {
      document.documentElement.style.setProperty('--font-scale', '1rem');
      localStorage.setItem('gov_font_scale', '1');
    });
  }

  if (btnIncr) {
    btnIncr.addEventListener('click', function () {
      let scale = parseFloat(localStorage.getItem('gov_font_scale') || '1');
      if (scale < 1.25) {
        scale = Math.round((scale + 0.05) * 100) / 100;
        document.documentElement.style.setProperty('--font-scale', scale + 'rem');
        localStorage.setItem('gov_font_scale', scale.toString());
      }
    });
  }

  // High contrast mode
  if (localStorage.getItem('gov_high_contrast') === 'true') {
    document.body.classList.add('high-contrast');
  }

  if (btnContrast) {
    btnContrast.addEventListener('click', function () {
      document.body.classList.toggle('high-contrast');
      const isHigh = document.body.classList.contains('high-contrast');
      localStorage.setItem('gov_high_contrast', isHigh ? 'true' : 'false');
    });
  }
}

// 2. Real-Time Double Entry Matching Validation
function initConfirmationMatchers() {
  const matchPairs = [
    { original: 'FirstName', confirm: 'ConfirmFirstName', label: 'First Name' },
    { original: 'MiddleName', confirm: 'ConfirmMiddleName', label: 'Middle Name' },
    { original: 'LastName', confirm: 'ConfirmLastName', label: 'Last Name' },
    { original: 'FatherGuardianName', confirm: 'ConfirmFatherGuardianName', label: 'Father/Guardian Name' },
    { original: 'MotherName', confirm: 'ConfirmMotherName', label: 'Mother Name' },
    { original: 'DateOfBirth', confirm: 'ConfirmDateOfBirth', label: 'Date of Birth' },
    { original: 'AadhaarNumber', confirm: 'ConfirmAadhaarNumber', label: 'Aadhaar Number' },
    { original: 'MobileNumber', confirm: 'ConfirmMobileNumber', label: 'Mobile Number' },
    { original: 'Email', confirm: 'ConfirmEmail', label: 'Email' },
    { original: 'AddressLine', confirm: 'ConfirmAddressLine', label: 'Address' },
    { original: 'Pincode', confirm: 'ConfirmPincode', label: 'PIN Code' },
    { original: 'Password', confirm: 'ConfirmPassword', label: 'Password' },
    { original: 'AccountNumber', confirm: 'ConfirmAccountNumber', label: 'Account Number' }
  ];

  matchPairs.forEach(pair => {
    const origEl = document.getElementById(pair.original);
    const confEl = document.getElementById(pair.confirm);

    if (origEl && confEl) {
      function checkMatch() {
        let feedback = confEl.parentNode.querySelector('.match-indicator');
        if (!feedback) {
          feedback = document.createElement('small');
          feedback.className = 'match-indicator';
          confEl.parentNode.appendChild(feedback);
        }

        if (!confEl.value) {
          feedback.innerHTML = '';
          return;
        }

        if (origEl.value.trim() === confEl.value.trim()) {
          feedback.innerHTML = '<i class="bi bi-check-circle-fill me-1"></i> ' + pair.label + ' matches correctly';
          feedback.className = 'match-indicator match-success';
        } else {
          feedback.innerHTML = '<i class="bi bi-exclamation-circle-fill me-1"></i> Does not match ' + pair.label;
          feedback.className = 'match-indicator match-fail';
        }
      }

      origEl.addEventListener('input', checkMatch);
      confEl.addEventListener('input', checkMatch);
    }
  });
}

// 3. Cascading Dependent Dropdowns
function initCascadingDropdowns() {
  const districtSelect = document.getElementById('DistrictId');
  if (districtSelect) {
    districtSelect.addEventListener('change', function () {
      const districtId = this.value;
      if (!districtId) {
        const resetIds = ['BlockId', 'VidhansabhaId', 'PostOfficeId', 'CityVillageId', 'InstituteId'];
        resetIds.forEach(id => {
          const el = document.getElementById(id);
          if (el) el.innerHTML = '<option value="">-- Select --</option>';
        });
        return;
      }

      loadDropdown('/api/masters/blocks?districtId=' + districtId, 'BlockId', 'Select Block');
      loadDropdown('/api/masters/vidhansabhas?districtId=' + districtId, 'VidhansabhaId', 'Select Vidhan Sabha');
      loadDropdown('/api/masters/post-offices?districtId=' + districtId, 'PostOfficeId', 'Select Post Office');
      loadDropdown('/api/masters/cities-villages?districtId=' + districtId, 'CityVillageId', 'Select City / Village');
      loadDropdown('/api/masters/institutes?districtId=' + districtId, 'InstituteId', 'Select Institute');
    });
  }

  const blockSelect = document.getElementById('BlockId');
  if (blockSelect) {
    blockSelect.addEventListener('change', function () {
      const blockId = this.value;
      const districtId = document.getElementById('DistrictId')?.value || '1';
      if (blockId) {
        loadDropdown('/api/masters/cities-villages?districtId=' + districtId + '&blockId=' + blockId, 'CityVillageId', 'Select City / Village');
      } else if (districtId) {
        loadDropdown('/api/masters/cities-villages?districtId=' + districtId, 'CityVillageId', 'Select City / Village');
      }
    });
  }

  const courseTypeSelect = document.getElementById('CourseTypeId');
  if (courseTypeSelect) {
    courseTypeSelect.addEventListener('change', function () {
      const typeId = this.value;
      loadDropdown('/api/masters/courses?courseTypeId=' + typeId, 'CourseId', 'Select Course');
    });
  }

  const courseSelect = document.getElementById('CourseId');
  if (courseSelect) {
    courseSelect.addEventListener('change', function () {
      const courseId = this.value;
      loadDropdown('/api/masters/course-branches?courseId=' + courseId, 'BranchId', 'Select Branch / Specialization');
    });
  }

  const bankSelect = document.getElementById('BankId');
  if (bankSelect) {
    bankSelect.addEventListener('change', function () {
      const bankId = this.value;
      loadDropdown('/api/masters/bank-branches?bankId=' + bankId, 'BranchId', 'Select Bank Branch');
    });
  }
}

function loadDropdown(url, elementId, placeholder, onLoaded) {
  const el = document.getElementById(elementId);
  if (!el) return;

  el.innerHTML = '<option value="">Loading official records...</option>';
  fetch(url)
    .then(r => r.json())
    .then(data => {
      let options = '<option value="">-- ' + placeholder + ' --</option>';
      const items = Array.isArray(data) ? data : (data?.data || data?.items || data?.value || []);
      items.forEach(item => {
        const id = item.id ?? item.blockId ?? item.vidhansabhaId ?? item.cityVillageId ?? item.postOfficeId ?? item.courseId ?? item.branchId ?? item.instituteId;
        const name = item.name ?? item.blockName ?? item.vidhansabhaName ?? item.postOfficeName ?? item.courseName ?? item.branchName ?? item.instituteName;
        if (id !== undefined && name !== undefined) {
          options += `<option value="${id}">${name}</option>`;
        }
      });
      el.innerHTML = options;
      if (typeof onLoaded === 'function') onLoaded(items);
    })
    .catch(err => {
      console.error('Error loading dropdown for ' + elementId, err);
      el.innerHTML = '<option value="">-- ' + placeholder + ' --</option>';
    });
}

// 4. Captcha Refresh Button with Smooth Animation
function initCaptchaRefresh() {
  const refreshBtn = document.getElementById('btnRefreshCaptcha');
  if (refreshBtn) {
    refreshBtn.addEventListener('click', function (e) {
      e.preventDefault();
      const icon = refreshBtn.querySelector('i');
      if (icon) icon.classList.add('bi-spin');

      fetch('/Account/RefreshCaptcha')
        .then(r => r.json())
        .then(data => {
          const textEl = document.getElementById('captchaDisplay');
          if (textEl) textEl.textContent = data.captcha;
          const tokenInput = document.getElementById('CaptchaToken');
          if (tokenInput) tokenInput.value = data.captcha;
          if (icon) icon.classList.remove('bi-spin');
        })
        .catch(() => {
          if (icon) icon.classList.remove('bi-spin');
        });
    });
  }
}

// 5. Application Lock OTP Modal & Dispatch
function initLockModal() {
  const btnSendOtp = document.getElementById('btnSendLockOtp');
  if (btnSendOtp) {
    btnSendOtp.addEventListener('click', function () {
      const appId = this.dataset.appId;
      btnSendOtp.disabled = true;
      btnSendOtp.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span> Dispatching OTP...';

      fetch('/Application/RequestLockOtp?applicationId=' + appId, { method: 'POST' })
        .then(r => r.json())
        .then(data => {
          btnSendOtp.disabled = false;
          btnSendOtp.innerHTML = '<i class="bi bi-arrow-repeat me-1"></i> Resend OTP';
          const msgEl = document.getElementById('otpFeedback');
          if (msgEl) {
            msgEl.className = data.success ? 'alert alert-success' : 'alert alert-danger';
            msgEl.innerHTML = `<i class="bi ${data.success ? 'bi-check-circle' : 'bi-exclamation-circle'} me-2"></i>` + data.message;
            msgEl.style.display = 'block';
          }
          if (data.success) {
            const inputGroup = document.getElementById('otpInputGroup');
            const submitBtn = document.getElementById('btnSubmitLock');
            if (inputGroup) inputGroup.style.display = 'block';
            if (submitBtn) submitBtn.disabled = false;
          }
        })
        .catch(() => {
          btnSendOtp.disabled = false;
          btnSendOtp.innerText = 'Resend OTP';
        });
    });
  }
}

// 6. Client-Side File Validation
function initFileUploadValidators() {
  document.querySelectorAll('input[type="file"]').forEach(input => {
    input.addEventListener('change', function () {
      const file = this.files[0];
      if (!file) return;

      const maxBytes = 2 * 1024 * 1024; // 2MB
      const allowedExts = ['.pdf', '.jpg', '.jpeg', '.png'];
      const ext = '.' + file.name.split('.').pop().toLowerCase();

      if (!allowedExts.includes(ext)) {
        alert('Invalid file format. Only PDF, JPG, and PNG are allowed according to Government e-Governance standards.');
        this.value = '';
        return;
      }

      if (file.size > maxBytes) {
        alert('File size exceeds the 2MB limit. Please compress the file before uploading.');
        this.value = '';
        return;
      }
    });
  });
}

// 7. Highlight Active Navigation Links
function highlightActiveNavLinks() {
  const currentPath = window.location.pathname.toLowerCase();
  document.querySelectorAll('.gov-nav-link').forEach(link => {
    const href = link.getAttribute('href')?.toLowerCase();
    if (href && (currentPath === href || (href !== '/' && currentPath.startsWith(href)))) {
      link.classList.add('active');
    }
  });
}
