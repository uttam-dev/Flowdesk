import { useRef, useState } from "react";
import { AlertCircle, FileSpreadsheet, Loader2, Upload } from "lucide-react";
import { Button } from "../../../components/ui/Button.jsx";
import { Modal } from "../../../components/ui/Modal.jsx";
import {
  Table,
  TableBody,
  TableHead,
  TableRow,
  Td,
  Th,
} from "../../../components/ui/Table.jsx";
import { bulkUploadUsersApi } from "../userApi.js";

const MAX_FILE_SIZE = 5 * 1024 * 1024;
const VALID_EXTENSIONS = [".csv", ".xlsx"];

const initialState = {
  step: "select",
  file: null,
  fileInfo: null,
  previewRows: [],
  rowCount: 0,
  parseError: "",
  parsed: false,
  isDragging: false,
  result: null,
  hardFailure: null,
};

function formatFileSize(size) {
  return `${Math.max(1, Math.round(size / 1024)).toLocaleString()} KB`;
}

function fileExtension(fileName) {
  const dot = fileName.lastIndexOf(".");
  return dot >= 0 ? fileName.slice(dot).toLowerCase() : "";
}

function normalizeHeader(value) {
  return String(value ?? "")
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9]/g, "");
}

function pickValue(row, candidates) {
  const keys = Object.keys(row ?? {});
  for (const key of keys) {
    if (candidates.includes(normalizeHeader(key))) {
      return row[key];
    }
  }
  return "";
}

function mapImportRow(row, index) {
  return {
    rowNumber: pickValue(row, ["rownumber"]) || (index + 2),
    fullName: pickValue(row, ["fullname", "name"]),
    email: pickValue(row, ["email", "emailaddress"]),
    roleName: pickValue(row, ["rolename", "role"]),
    managerEmail: pickValue(row, ["manageremail"]),
    isActive: pickValue(row, ["isactive", "active"]),
  };
}

function hasMissingRequired(row) {
  return !String(row.fullName ?? "").trim()
    || !String(row.email ?? "").trim()
    || !String(row.roleName ?? "").trim();
}

function displayValue(value) {
  if (value === false) return "false";
  if (value === 0) return "0";
  return value || "—";
}

function validateFile(file) {
  const extension = fileExtension(file.name);
  if (!VALID_EXTENSIONS.includes(extension)) {
    return "Only CSV or Excel files are allowed.";
  }
  if (file.size > MAX_FILE_SIZE) {
    return "File exceeds the 5 MB limit.";
  }
  return "";
}

function parseCsv(file) {
  return new Promise((resolve, reject) => {
    import("papaparse")
      .then(({ default: Papa }) => {
        Papa.parse(file, {
          header: true,
          skipEmptyLines: "greedy",
          complete: (results) => {
            if (results.errors?.length) {
              reject(new Error(results.errors[0].message));
              return;
            }
            resolve((results.data ?? []).filter(Boolean));
          },
          error: (error) => reject(error),
        });
      })
      .catch(reject);
  });
}

async function parseExcel(file) {
  const XLSX = await import("xlsx");
  const buffer = await file.arrayBuffer();
  const workbook = XLSX.read(buffer, { type: "array" });
  const firstSheet = workbook.SheetNames[0];
  if (!firstSheet) return [];
  return XLSX.utils.sheet_to_json(workbook.Sheets[firstSheet], {
    defval: "",
    blankrows: false,
  });
}

async function parseFile(file) {
  const extension = fileExtension(file.name);
  const rows = extension === ".csv" ? await parseCsv(file) : await parseExcel(file);
  return rows.map(mapImportRow);
}

function getApiStatus(payload, fallbackStatus) {
  return Number(payload?.statusCode ?? payload?.StatusCode ?? fallbackStatus ?? 0);
}

function normalizeSuccessPayload(payload) {
  const data = payload?.data ?? payload?.Data ?? {};
  const errors = data.errors ?? data.Errors ?? [];
  return {
    total: Number(data.total ?? data.Total ?? 0),
    successCount: Number(data.successCount ?? data.SuccessCount ?? 0),
    failedCount: Number(data.failedCount ?? data.FailedCount ?? 0),
    errors: Array.isArray(errors)
      ? errors.map((e) => ({
          rowNumber: e.rowNumber ?? e.RowNumber ?? "",
          email: e.email ?? e.Email ?? "",
          error: e.error ?? e.Error ?? e.reason ?? e.Reason ?? "",
        }))
      : [],
  };
}

function normalizeHardFailure(payload) {
  return {
    message: payload?.message ?? payload?.Message ?? "Bulk upload failed.",
    errorCode: payload?.errorCode ?? payload?.ErrorCode ?? "UNKNOWN",
    traceId: payload?.traceId ?? payload?.TraceId ?? "—",
  };
}

function escapeCsv(value) {
  const text = String(value ?? "");
  if (/[",\n\r]/.test(text)) {
    return `"${text.replace(/"/g, '""')}"`;
  }
  return text;
}

function downloadErrors(errors) {
  const header = ["Row", "Email", "Reason"];
  const lines = [
    header.map(escapeCsv).join(","),
    ...errors.map((e) =>
      [e.rowNumber, e.email, e.error].map(escapeCsv).join(","),
    ),
  ];
  const blob = new Blob([lines.join("\n")], {
    type: "text/csv;charset=utf-8",
  });
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = "bulk_upload_errors.csv";
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}

function MetricCard({ label, value, tone = "neutral" }) {
  const toneClass = {
    neutral: "text-gray-900",
    success: "text-emerald-700",
    danger: "text-red-700",
  }[tone];

  return (
    <div className="rounded-lg border border-gray-200 bg-white px-4 py-3 shadow-sm">
      <p className="text-xs font-medium uppercase tracking-wide text-gray-500">
        {label}
      </p>
      <p className={`mt-1 text-2xl font-semibold ${toneClass}`}>{value}</p>
    </div>
  );
}

function ErrorAlert({ failure, network }) {
  return (
    <div
      className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800"
      role="alert"
    >
      <div className="flex gap-2">
        <AlertCircle className="mt-0.5 h-4 w-4 shrink-0" aria-hidden />
        <div className="space-y-1">
          <p className="font-semibold">
            {network
              ? "Unable to reach the server. Please check your connection and try again."
              : failure.message}
          </p>
          {!network ? (
            <>
              <p>Error code: {failure.errorCode}</p>
              <p>Trace ID: {failure.traceId}</p>
            </>
          ) : null}
        </div>
      </div>
    </div>
  );
}

export function BulkUploadModal({ open, onClose, onCompleted }) {
  const inputRef = useRef(null);
  const [state, setState] = useState(initialState);

  if (!open) return null;

  const isProcessing = state.step === "processing";
  const canUpload = state.file && state.parsed && !state.parseError;
  const successData = state.result?.data;
  const successPercent =
    successData?.total > 0
      ? Math.min(100, (successData.successCount / successData.total) * 100)
      : 0;

  function downloadTemplate(e) {
    e.stopPropagation();
    const content = "RowNumber,FullName,Email,RoleName,ManagerEmail,IsActive\n1,John Doe,john.doe@company.com,Admin,,true\n2,Jane Smith,jane.smith@company.com,Employee,john.doe@company.com,true";
    const blob = new Blob([content], { type: "text/csv;charset=utf-8" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = "bulk_users_template.csv";
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  }

  function reset() {
    setState(initialState);
    if (inputRef.current) inputRef.current.value = "";
  }

  function closeModal() {
    if (isProcessing) return;
    reset();
    onClose?.();
  }

  async function handleFile(file) {
    if (!file) return;
    const validationError = validateFile(file);
    if (validationError) {
      setState((prev) => ({
        ...prev,
        file: null,
        fileInfo: null,
        previewRows: [],
        rowCount: 0,
        parseError: validationError,
        parsed: false,
      }));
      return;
    }

    setState((prev) => ({
      ...prev,
      file,
      fileInfo: null,
      previewRows: [],
      rowCount: 0,
      parseError: "",
      parsed: false,
    }));

    try {
      const rows = await parseFile(file);
      setState((prev) => ({
        ...prev,
        file,
        fileInfo: {
          name: file.name,
          size: formatFileSize(file.size),
        },
        previewRows: rows.slice(0, 5),
        rowCount: rows.length,
        parseError: "",
        parsed: true,
      }));
    } catch {
      setState((prev) => ({
        ...prev,
        file: null,
        fileInfo: null,
        previewRows: [],
        rowCount: 0,
        parseError: "Unable to parse the selected file.",
        parsed: false,
      }));
    }
  }

  async function uploadFile() {
    if (!state.file || isProcessing) return;
    setState((prev) => ({ ...prev, step: "processing" }));

    try {
      const response = await bulkUploadUsersApi(state.file);
      const statusCode = getApiStatus(response, 200);
      if (statusCode === 200) {
        const data = normalizeSuccessPayload(response);
        setState((prev) => ({
          ...prev,
          step: "results",
          result: { statusCode, data },
          hardFailure: null,
        }));
        onCompleted?.();
      } else {
        setState((prev) => ({
          ...prev,
          step: "results",
          result: null,
          hardFailure: normalizeHardFailure(response),
        }));
      }
    } catch (error) {
      const response = error?.response;
      const payload = response?.data;
      if (!response) {
        setState((prev) => ({
          ...prev,
          step: "results",
          result: null,
          hardFailure: {
            message:
              "Unable to reach the server. Please check your connection and try again.",
            errorCode: "",
            traceId: "",
            network: true,
          },
        }));
        return;
      }

      setState((prev) => ({
        ...prev,
        step: "results",
        result: null,
        hardFailure: normalizeHardFailure(payload),
      }));
    }
  }

  const footer =
    state.step === "processing" ? null : state.step === "results" ? (
      state.result ? (
        <>
          <Button type="button" variant="secondary" onClick={closeModal}>
            Close
          </Button>
          <Button type="button" variant="primary" onClick={reset}>
            Upload another
          </Button>
        </>
      ) : (
        <>
          <Button type="button" variant="secondary" onClick={closeModal}>
            Close
          </Button>
          <Button type="button" variant="primary" onClick={reset}>
            Try again
          </Button>
        </>
      )
    ) : (
      <>
        <Button type="button" variant="secondary" onClick={closeModal}>
          Cancel
        </Button>
        <Button type="button" variant="primary" disabled={!canUpload} onClick={uploadFile}>
          Upload
        </Button>
      </>
    );

  return (
    <Modal
      open
      onClose={closeModal}
      title="Bulk upload users"
      size="lg"
      closeOnOverlayClick={!isProcessing}
      closeOnEscape={!isProcessing}
      footer={footer}
    >
      {state.step === "select" ? (
        <div className="space-y-4">
          <div
            className={`rounded-lg border-2 border-dashed px-5 py-8 text-center transition ${
              state.isDragging
                ? "border-blue-400 bg-blue-50"
                : "border-gray-300 bg-gray-50"
            }`}
            onDragOver={(event) => {
              event.preventDefault();
              setState((prev) => ({ ...prev, isDragging: true }));
            }}
            onDragLeave={() =>
              setState((prev) => ({ ...prev, isDragging: false }))
            }
            onDrop={(event) => {
              event.preventDefault();
              setState((prev) => ({ ...prev, isDragging: false }));
              handleFile(event.dataTransfer.files?.[0]);
            }}
          >
            <input
              ref={inputRef}
              type="file"
              accept=".csv,.xlsx"
              className="sr-only"
              onChange={(event) => handleFile(event.target.files?.[0])}
            />
            <div className="mx-auto flex h-11 w-11 items-center justify-center rounded-full bg-white text-emerald-700 ring-1 ring-gray-200">
              <Upload className="h-5 w-5" aria-hidden />
            </div>
            <p className="mt-3 text-sm font-semibold text-gray-900">
              Drop a CSV or Excel file here
            </p>
            <p className="mt-1 text-xs text-gray-500">Maximum file size: 5 MB</p>
            <div className="mt-2">
              <button
                type="button"
                className="inline-flex items-center gap-1.5 text-xs font-medium text-emerald-700 hover:text-emerald-800 hover:underline"
                onClick={downloadTemplate}
              >
                <i className="ti-download" aria-hidden="true" />
                Download CSV template
              </button>
            </div>
            <Button
              type="button"
              variant="secondary"
              className="mt-4 min-h-0 px-3 py-2 text-xs"
              onClick={() => inputRef.current?.click()}
            >
              Browse file
            </Button>
          </div>

          {state.parseError ? (
            <p className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">
              {state.parseError}
            </p>
          ) : null}

          {state.fileInfo ? (
            <>
              <div className="flex flex-col gap-3 rounded-lg border border-gray-200 bg-white px-4 py-3 sm:flex-row sm:items-center sm:justify-between">
                <div className="flex min-w-0 items-center gap-3">
                  <FileSpreadsheet className="h-5 w-5 shrink-0 text-emerald-700" aria-hidden />
                  <div className="min-w-0">
                    <p className="truncate text-sm font-semibold text-gray-900">
                      {state.fileInfo.name}
                    </p>
                    <p className="text-xs text-gray-500">
                      {state.fileInfo.size} · {state.rowCount.toLocaleString()} data rows
                    </p>
                  </div>
                </div>
                <span className="inline-flex w-fit items-center rounded-full bg-emerald-50 px-2.5 py-1 text-xs font-semibold text-emerald-800 ring-1 ring-inset ring-emerald-100">
                  {state.rowCount.toLocaleString()} users detected
                </span>
              </div>

              <div className="max-h-64 overflow-auto">
                <Table>
                  <TableHead>
                    <TableRow className="hover:bg-transparent">
                      <Th>Row</Th>
                      <Th>Full name</Th>
                      <Th>Email</Th>
                      <Th>Role</Th>
                      <Th>Manager email</Th>
                      <Th>Active</Th>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {state.previewRows.length ? (
                      state.previewRows.map((row) => {
                        let isActiveDisplay = "Yes";
                        const activeStr = String(row.isActive ?? "").trim().toLowerCase();
                        if (activeStr === "false" || activeStr === "no" || activeStr === "0") {
                          isActiveDisplay = "No";
                        }

                        return (
                          <TableRow
                            key={row.rowNumber}
                            className={hasMissingRequired(row) ? "bg-red-50" : "bg-white"}
                          >
                            <Td>{row.rowNumber}</Td>
                            <Td>{displayValue(row.fullName)}</Td>
                            <Td>{displayValue(row.email)}</Td>
                            <Td>{displayValue(row.roleName)}</Td>
                            <Td>{displayValue(row.managerEmail)}</Td>
                            <Td>{isActiveDisplay}</Td>
                          </TableRow>
                        );
                      })
                    ) : (
                      <TableRow>
                        <Td colSpan={6} className="py-6 text-center text-gray-500">
                          No preview rows found.
                        </Td>
                      </TableRow>
                    )}
                  </TableBody>
                </Table>
              </div>

              <p className="rounded-lg border border-blue-200 bg-blue-50 px-3 py-2 text-sm text-blue-800">
                Review the data above. {state.rowCount.toLocaleString()} users will be submitted for import.
              </p>
            </>
          ) : null}
        </div>
      ) : null}

      {state.step === "processing" ? (
        <div className="space-y-5 py-8 text-center">
          <Loader2 className="mx-auto h-10 w-10 animate-spin text-emerald-700" aria-hidden />
          <div>
            <h3 className="text-lg font-semibold text-gray-900">
              Processing users…
            </h3>
            <p className="mt-1 text-sm text-gray-600">
              Please wait while we validate and insert your records.
            </p>
          </div>
          <div className="h-2 overflow-hidden rounded-full bg-gray-100">
            <div className="h-full w-1/2 animate-pulse rounded-full bg-emerald-600" />
          </div>
        </div>
      ) : null}

      {state.step === "results" ? (
        <div className="space-y-4">
          {state.result ? (
            <>
              <div className="grid gap-3 sm:grid-cols-3">
                <MetricCard label="Total rows" value={successData.total.toLocaleString()} />
                <MetricCard
                  label="Inserted"
                  value={successData.successCount.toLocaleString()}
                  tone="success"
                />
                <MetricCard
                  label="Failed"
                  value={successData.failedCount.toLocaleString()}
                  tone={successData.failedCount > 0 ? "danger" : "neutral"}
                />
              </div>
              <div className="h-2 overflow-hidden rounded-full bg-gray-100">
                <div
                  className={`h-full rounded-full ${
                    successData.failedCount > 0 ? "bg-amber-500" : "bg-emerald-600"
                  }`}
                  style={{ width: `${successPercent}%` }}
                />
              </div>
              <p
                className={`rounded-lg border px-3 py-2 text-sm ${
                  successData.failedCount === 0
                    ? "border-emerald-200 bg-emerald-50 text-emerald-800"
                    : "border-amber-200 bg-amber-50 text-amber-800"
                }`}
              >
                {successData.failedCount === 0
                  ? `All ${successData.total.toLocaleString()} users imported successfully.`
                  : `${successData.successCount.toLocaleString()} users inserted. ${successData.failedCount.toLocaleString()} rows failed — review the errors below.`}
              </p>

              {successData.errors.length ? (
                <div className="space-y-3">
                  <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
                    <span className="inline-flex w-fit items-center rounded-full bg-red-50 px-2.5 py-1 text-xs font-semibold text-red-800 ring-1 ring-inset ring-red-100">
                      {successData.errors.length.toLocaleString()} errors
                    </span>
                    <Button
                      type="button"
                      variant="secondary"
                      className="min-h-0 px-3 py-2 text-xs"
                      onClick={() => downloadErrors(successData.errors)}
                    >
                      Download error report
                    </Button>
                  </div>
                  <div className="max-h-56 overflow-auto">
                    <Table>
                      <TableHead>
                        <TableRow className="bg-red-50 hover:bg-red-50">
                          <Th className="text-red-800">Row</Th>
                          <Th className="text-red-800">Email</Th>
                          <Th className="text-red-800">Reason</Th>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {successData.errors.map((error, index) => (
                          <TableRow
                            key={`${error.rowNumber}-${error.email}-${index}`}
                            className="bg-red-50/70"
                          >
                            <Td className="text-red-900">
                              {displayValue(error.rowNumber)}
                            </Td>
                            <Td className="text-red-900">
                              {displayValue(error.email)}
                            </Td>
                            <Td className="text-red-900">
                              {displayValue(error.error)}
                            </Td>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </div>
                </div>
              ) : null}
            </>
          ) : (
            <>
              <ErrorAlert failure={state.hardFailure} network={state.hardFailure?.network} />
              <div className="grid gap-3 sm:grid-cols-3">
                <MetricCard label="Total rows" value="—" />
                <MetricCard label="Inserted" value="—" />
                <MetricCard label="Failed" value="—" />
              </div>
            </>
          )}
        </div>
      ) : null}
    </Modal>
  );
}
