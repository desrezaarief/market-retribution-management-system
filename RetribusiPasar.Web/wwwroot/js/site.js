(function () {
  const rupiah = value => new Intl.NumberFormat("id-ID", {
    style: "currency",
    currency: "IDR",
    maximumFractionDigits: 0
  }).format(Number(value || 0));

  const digits = value => String(value ?? "").replace(/\D/g, "");
  const thousand = value => {
    const d = digits(value);
    return d ? d.replace(/\B(?=(\d{3})+(?!\d))/g, ".") : "";
  };

  function showLoading(title = "Memproses...", text = "Mohon tunggu sebentar.") {
    const overlay = document.getElementById("globalLoadingOverlay");
    if (!overlay) return;
    const titleEl = document.getElementById("globalLoadingTitle");
    const textEl = document.getElementById("globalLoadingText");
    if (titleEl) titleEl.textContent = title;
    if (textEl) textEl.textContent = text;
    overlay.hidden = false;
  }

  function hideLoading() {
    const overlay = document.getElementById("globalLoadingOverlay");
    if (overlay) overlay.hidden = true;
  }

  function formatNumberInput(el) {
    if (!el) return;
    el.value = thousand(el.value);
  }

  function initNumericInputs(container = document) {
    container.querySelectorAll("[data-number-format]").forEach(el => {
      formatNumberInput(el);
      if (el.dataset.numberBound) return;
      el.dataset.numberBound = "1";
      el.addEventListener("input", () => formatNumberInput(el));
      el.addEventListener("paste", () => setTimeout(() => formatNumberInput(el), 0));
    });

    container.querySelectorAll("[data-digits-only]").forEach(el => {
      el.value = digits(el.value);
      if (el.dataset.digitsBound) return;
      el.dataset.digitsBound = "1";
      el.addEventListener("input", () => { el.value = digits(el.value); });
      el.addEventListener("paste", () => setTimeout(() => { el.value = digits(el.value); }, 0));
    });
  }

  function normalizeNumericFields(form) {
    form.querySelectorAll("[data-number-format]").forEach(el => {
      el.value = digits(el.value);
    });
    form.querySelectorAll("[data-digits-only]").forEach(el => {
      el.value = digits(el.value);
    });
  }

  function initSelect2(container = document) {
    if (!window.jQuery || !window.jQuery.fn || !window.jQuery.fn.select2) return;
    const $ = window.jQuery;

    $(container).find("select").each(function () {
      const $select = $(this);
      if ($select.hasClass("select2-hidden-accessible")) return;

      const firstOption = this.options && this.options.length ? this.options[0] : null;
      const placeholder = firstOption && !firstOption.value ? firstOption.text : "Pilih data";
      const inModal = !!this.closest("#appModalBody");
      const options = {
        width: "100%",
        placeholder: placeholder,
        minimumResultsForSearch: 0
      };

      if (inModal) options.dropdownParent = $("#appModal .modal-card");
      $select.select2(options);
      // Bridge Select2 events to native change listeners used by the receipt form.
      $select.on("select2:select select2:clear", function () {
        this.dispatchEvent(new Event("change", { bubbles: true }));
      });
    });
  }

  function syncSelect2(el) {
    if (!el || !window.jQuery || !window.jQuery.fn?.select2) return;
    const $el = window.jQuery(el);
    if ($el.hasClass("select2-hidden-accessible")) $el.trigger("change.select2");
  }

  function initConfirm(container = document) {
    container.querySelectorAll("[data-confirm]").forEach(el => {
      if (el.dataset.confirmBound) return;
      el.dataset.confirmBound = "1";
      el.addEventListener("click", e => {
        if (!confirm(el.dataset.confirm || "Lanjutkan?")) e.preventDefault();
      });
    });
  }

  function initStandardFormLoading(container = document) {
    container.querySelectorAll("form").forEach(form => {
      if (form.dataset.loadingBound || form.closest("#appModalBody")) return;
      form.dataset.loadingBound = "1";
      form.addEventListener("submit", () => {
        if (typeof form.checkValidity === "function" && !form.checkValidity()) return;
        normalizeNumericFields(form);
        const method = (form.method || "get").toLowerCase();
        showLoading(
          method === "get" ? "Memuat data..." : "Menyimpan data...",
          method === "get" ? "Sedang mengambil data terbaru." : "Perubahan sedang diproses oleh sistem."
        );
      });
    });
  }

  function initDepositForm(container = document) {
    const checks = container.querySelectorAll(".deposit-receipt");
    const totalEl = container.querySelector("#depositCalculatedTotal");
    if (!checks.length || !totalEl) return;

    const totalDeposit = () => {
      let total = 0;
      checks.forEach(x => { if (x.checked) total += Number(x.dataset.amount || 0); });
      totalEl.textContent = rupiah(total);
    };

    checks.forEach(x => {
      if (x.dataset.depositBound) return;
      x.dataset.depositBound = "1";
      x.addEventListener("change", totalDeposit);
    });
    totalDeposit();
  }

  function initReceiptForm(container = document) {
    container.querySelectorAll("form").forEach(form => {
      const unit = form.querySelector("#UnitId");
      const marketSelect = form.querySelector("#MarketSelect");
      const typeSelect = form.querySelector("#RetributionTypeSelect");
      const collectorSelect = form.querySelector("#CollectorSelect");
      if (!unit || !marketSelect || !typeSelect || form.dataset.receiptInit) return;
      form.dataset.receiptInit = "1";

      const marketId = form.querySelector("#MarketId");
      const typeId = form.querySelector("#RetributionTypeId");
      const collectorId = form.querySelector("#CollectorId");
      const period = form.querySelector("#PeriodStart");
      const months = form.querySelector("#Months");
      const startSerial = form.querySelector("#StartSerial");
      const endSerial = form.querySelector("#EndSerial");
      const media = form.querySelector("#MediaTypeDisplay");
      const tariff = form.querySelector("#TariffDisplay");
      const expected = form.querySelector("#ExpectedAmount");
      const received = form.querySelector("#ReceivedAmount");
      const ranges = form.querySelector("#AvailableSerialRanges");
      const serialSection = form.querySelector("#SerialSection");
      const receiptId = form.querySelector("#ReceiptId");
      const formConfig = form.querySelector("#ReceiptFormConfig");
      const contextUrl = formConfig?.dataset.contextUrl || "/Receipts/FormContext";
      const marketLockInfo = form.querySelector("#MarketLockInfo");
      const typeLockInfo = form.querySelector("#TypeLockInfo");
      const collectorLockInfo = form.querySelector("#CollectorLockInfo");

      const setSelectValue = (select, value) => {
        if (!select) return;
        const normalized = value === null || value === undefined ? "" : String(value);
        select.value = normalized;

        if (window.jQuery && window.jQuery.fn?.select2) {
          const $select = window.jQuery(select);
          if ($select.hasClass("select2-hidden-accessible")) {
            $select.val(normalized).trigger("change.select2");
          }
        }
      };

      const syncDisplaySelectsFromHidden = () => {
        setSelectValue(marketSelect, marketId?.value || "");
        setSelectValue(typeSelect, typeId?.value || "");
        setSelectValue(collectorSelect, collectorId?.value || "");
      };

      const syncHiddenFromDisplaySelects = () => {
        if (marketId) marketId.value = marketSelect.value || "0";
        if (typeId) typeId.value = typeSelect.value || "0";
        if (collectorId) collectorId.value = collectorSelect.value || "";
      };

      const lockByUnit = () => {
        const locked = !!unit.value;
        marketSelect.disabled = locked;
        typeSelect.disabled = locked;
        collectorSelect.disabled = locked;
        syncSelect2(marketSelect);
        syncSelect2(typeSelect);
        syncSelect2(collectorSelect);
        if (marketLockInfo) marketLockInfo.textContent = locked ? "Mengikuti unit." : "";
        if (typeLockInfo) typeLockInfo.textContent = locked ? "Mengikuti unit." : "";
        if (collectorLockInfo) collectorLockInfo.textContent = locked ? "Mengikuti unit." : "";
      };

      async function refreshReceiptContext(autoFillReceived = false) {
        const hasUnit = !!unit.value;

        // Untuk transaksi tanpa unit, Pasar + Jenis harus dipilih manual.
        // Untuk transaksi dengan unit, jangan berhenti meskipun hidden Market/Type masih kosong,
        // karena backend FormContext akan menurunkannya dari master unit.
        if (!hasUnit) {
          syncHiddenFromDisplaySelects();

          if ((!marketId?.value || marketId.value === "0") || (!typeId?.value || typeId.value === "0")) {
            if (media) media.value = "-";
            if (tariff) tariff.value = rupiah(0);
            if (expected) { expected.value = "0"; formatNumberInput(expected); }
            if (serialSection) serialSection.style.display = "";
            if (ranges) ranges.textContent = "Pilih Pasar dan Jenis Retribusi.";
            return;
          }
        }

        const q = new URLSearchParams({
          marketId: marketId.value || "",
          retributionTypeId: typeId.value || "",
          collectorId: collectorId?.value || "",
          periodStart: period?.value || "",
          months: digits(months?.value || "1") || "1",
          startSerial: digits(startSerial?.value || ""),
          endSerial: digits(endSerial?.value || ""),
          receiptId: receiptId?.value || "0"
        });
        if (unit.value) q.set("unitId", unit.value);

        try {
          const res = await fetch(contextUrl + "?" + q.toString(), {
            headers: { "X-Requested-With": "XMLHttpRequest" },
            credentials: "same-origin"
          });
          if (!res.ok) return;
          const data = await res.json();
          if (!data.success) return;

          if (marketId) marketId.value = String(data.marketId ?? "");
          if (typeId) typeId.value = String(data.retributionTypeId ?? "");
          if (collectorId) collectorId.value = data.collectorId ? String(data.collectorId) : "";

          // Update native select + tampilan Select2 secara eksplisit.
          setSelectValue(marketSelect, data.marketId);
          setSelectValue(typeSelect, data.retributionTypeId);
          setSelectValue(collectorSelect, data.collectorId || "");
          lockByUnit();

          if (media) media.value = data.mediaType || "-";
          if (tariff) tariff.value = rupiah(data.tariff);
          if (expected) {
            expected.value = String(Math.round(Number(data.expectedAmount || 0)));
            formatNumberInput(expected);
          }
          if (received && (autoFillReceived || !digits(received.value) || Number(digits(received.value)) === 0)) {
            received.value = String(Math.round(Number(data.expectedAmount || 0)));
            formatNumberInput(received);
          }

          if (data.mediaType === "Tanpa Media") {
            if (serialSection) serialSection.style.display = "none";
            if (startSerial) startSerial.value = "";
            if (endSerial) endSerial.value = "";
            if (ranges) ranges.textContent = "Jenis retribusi ini tidak menggunakan media bernomor seri.";
          } else {
            if (serialSection) serialSection.style.display = "";
            const list = data.availableRanges || [];
            if (ranges) {
              ranges.innerHTML = list.length
                ? list.map(x => `<span class="tag ok serial-range-tag" data-start="${x.startSerial}" data-end="${x.endSerial}" title="Klik untuk memilih nomor pertama dari range">${x.label} · ${Number(x.totalSheets).toLocaleString("id-ID")} lembar</span>`).join(" ")
                : `<span class="tag bad">Tidak ada range ${data.mediaType} yang tersedia untuk pasar ini.</span>`;
            }
          }
        } catch (err) {
          console.error("Receipt context error", err);
        }
      }

      unit.addEventListener("change", async () => {
        lockByUnit();

        if (!unit.value) {
          // Unit dilepas: kembalikan mode input manual.
          if (marketId) marketId.value = marketSelect.value || "0";
          if (typeId) typeId.value = typeSelect.value || "0";
          if (collectorId) collectorId.value = collectorSelect.value || "";
        }

        await refreshReceiptContext(true);
      });
      marketSelect.addEventListener("change", () => { syncHiddenFromDisplaySelects(); refreshReceiptContext(true); });
      typeSelect.addEventListener("change", () => { syncHiddenFromDisplaySelects(); refreshReceiptContext(true); });
      collectorSelect.addEventListener("change", syncHiddenFromDisplaySelects);
      period?.addEventListener("change", () => refreshReceiptContext(true));
      months?.addEventListener("input", () => refreshReceiptContext(true));
      startSerial?.addEventListener("input", () => refreshReceiptContext(false));
      endSerial?.addEventListener("input", () => refreshReceiptContext(false));
      ranges?.addEventListener("click", e => {
        const tag = e.target.closest(".serial-range-tag");
        if (!tag) return;
        if (startSerial) startSerial.value = String(tag.dataset.start).padStart(6, "0");
        if (endSerial) endSerial.value = String(tag.dataset.start).padStart(6, "0");
        refreshReceiptContext(true);
      });

      syncDisplaySelectsFromHidden();
      lockByUnit();
      refreshReceiptContext(false);
    });
  }

  function parseModalContent(htmlText, sourceUrl) {
    const parser = new DOMParser();
    const doc = parser.parseFromString(htmlText, "text/html");
    const title = doc.querySelector(".page-header h1, .topbar h1, title")?.textContent?.trim() || "Form";
    const sourceNode = doc.querySelector(".panel") || doc.querySelector("main") || doc.body;
    if (!sourceNode) return { title, html: htmlText };

    const node = sourceNode.cloneNode(true);
    const actionUrl = new URL(sourceUrl, window.location.origin);
    node.querySelectorAll("form").forEach(form => {
      const rawAction = form.getAttribute("action");
      if (!rawAction || rawAction === "#") {
        form.setAttribute("action", actionUrl.pathname + actionUrl.search);
      }
    });
    return { title, html: node.outerHTML };
  }

  function initializeComponents(container = document) {
    initConfirm(container);
    initNumericInputs(container);
    initDepositForm(container);
    initReceiptForm(container);
    initSelect2(container);
  }

  function setupModalContent(modalBody, sourceUrl) {
    initializeComponents(modalBody);

    modalBody.querySelectorAll(".js-modal-close, a.btn.secondary").forEach(a => {
      if (a.dataset.closeBound) return;
      a.dataset.closeBound = "1";
      a.addEventListener("click", e => {
        const href = a.getAttribute("href") || "";
        if (a.classList.contains("js-modal-close") || /Index$/i.test(href) || a.textContent.trim().toLowerCase() === "batal") {
          e.preventDefault();
          closeModal();
        }
      });
    });

    const form = modalBody.querySelector("form");
    if (!form || form.dataset.modalSubmitBound) return;
    form.dataset.modalSubmitBound = "1";

    form.addEventListener("submit", async e => {
      e.preventDefault();
      if (typeof form.reportValidity === "function" && !form.reportValidity()) return;

      normalizeNumericFields(form);
      showLoading("Menyimpan data...", "Sedang memproses perubahan.");

      try {
        const action = form.getAttribute("action") || sourceUrl || window.location.href;
        const response = await fetch(action, {
          method: (form.method || "post").toUpperCase(),
          body: new FormData(form),
          headers: { "X-Requested-With": "XMLHttpRequest" },
          credentials: "same-origin",
          redirect: "follow"
        });

        const responseUrl = response.url || action;
        const text = await response.text();

        if (response.redirected) {
          closeModal();
          showLoading("Memuat ulang...", "Data berhasil diproses.");
          window.location.reload();
          return;
        }

        const parsed = parseModalContent(text, responseUrl);
        const modalTitle = document.getElementById("appModalTitle");
        if (modalTitle) modalTitle.textContent = parsed.title;
        modalBody.innerHTML = parsed.html;
        setupModalContent(modalBody, responseUrl);
      } catch (err) {
        console.error("Modal submit error", err);
        alert("Terjadi kesalahan saat menyimpan data. Silakan coba kembali.");
        initNumericInputs(form);
      } finally {
        hideLoading();
      }
    });
  }

  async function openModal(url) {
    const modal = document.getElementById("appModal");
    const title = document.getElementById("appModalTitle");
    const body = document.getElementById("appModalBody");
    if (!modal || !title || !body) {
      window.location.href = url;
      return;
    }

    showLoading("Memuat form...", "Sedang menyiapkan data.");
    try {
      const response = await fetch(url, {
        headers: { "X-Requested-With": "XMLHttpRequest" },
        credentials: "same-origin",
        redirect: "follow"
      });
      const text = await response.text();
      const sourceUrl = response.url || url;
      const parsed = parseModalContent(text, sourceUrl);
      title.textContent = parsed.title;
      body.innerHTML = parsed.html;
      modal.hidden = false;
      document.body.classList.add("modal-open");
      setupModalContent(body, sourceUrl);
    } catch (err) {
      console.error("Modal load error", err);
      window.location.href = url;
    } finally {
      hideLoading();
    }
  }

  function closeModal() {
    const modal = document.getElementById("appModal");
    const body = document.getElementById("appModalBody");
    if (window.jQuery && window.jQuery.fn?.select2 && body) {
      window.jQuery(body).find("select.select2-hidden-accessible").each(function () {
        window.jQuery(this).select2("destroy");
      });
    }
    if (body) body.innerHTML = "";
    if (modal) modal.hidden = true;
    document.body.classList.remove("modal-open");
  }

  function initModalLinks(container = document) {
    container.querySelectorAll(".js-modal-link").forEach(link => {
      if (link.dataset.modalBound) return;
      link.dataset.modalBound = "1";
      link.addEventListener("click", e => {
        e.preventDefault();
        openModal(link.href);
      });
    });
  }

  function initSidebarScrollPersistence() {
    const sidebar = document.querySelector(".side");
    if (!sidebar) return;

    const key = "rp-sidebar-scroll-top";
    const restore = () => {
      const saved = sessionStorage.getItem(key);
      if (saved !== null) sidebar.scrollTop = Number(saved) || 0;
    };

    // Two frames make sure sticky/sidebar layout has finished calculating height.
    requestAnimationFrame(() => requestAnimationFrame(restore));

    let timer;
    sidebar.addEventListener("scroll", () => {
      clearTimeout(timer);
      timer = setTimeout(() => {
        sessionStorage.setItem(key, String(sidebar.scrollTop));
      }, 50);
    }, { passive: true });

    sidebar.querySelectorAll("a").forEach(link => {
      link.addEventListener("click", () => {
        sessionStorage.setItem(key, String(sidebar.scrollTop));
      });
    });

    window.addEventListener("beforeunload", () => {
      sessionStorage.setItem(key, String(sidebar.scrollTop));
    });
  }

  function initThemeToggle() {
    const button = document.getElementById("themeToggle");
    const root = document.documentElement;
    if (!button) return;

    const setButtonHint = theme => {
      const dark = theme === "dark";
      button.setAttribute("aria-label", dark ? "Aktifkan mode terang" : "Aktifkan mode gelap");
      button.setAttribute("title", dark ? "Mode terang" : "Mode gelap");
    };

    setButtonHint(root.dataset.theme || "light");

    button.addEventListener("click", () => {
      const next = root.dataset.theme === "dark" ? "light" : "dark";
      root.dataset.theme = next;
      localStorage.setItem("rp-theme", next);
      setButtonHint(next);
      window.dispatchEvent(new CustomEvent("rp-theme-change", { detail: { theme: next } }));
    });
  }

  function initCloseHooks() {
    document.getElementById("appModalClose")?.addEventListener("click", closeModal);
    document.getElementById("appModal")?.addEventListener("click", e => {
      if (e.target.id === "appModal") closeModal();
    });
    document.addEventListener("keydown", e => {
      if (e.key === "Escape") closeModal();
    });
  }

  document.addEventListener("DOMContentLoaded", () => {
    initCloseHooks();
    initSidebarScrollPersistence();
    initThemeToggle();
    initializeComponents(document);
    initModalLinks(document);
    initStandardFormLoading(document);
    hideLoading();
  });

  window.addEventListener("pageshow", hideLoading);
  window.addEventListener("load", hideLoading);
})();
