import { Upload } from "lucide-react";
import { Button } from "../../../components/ui/Button.jsx";

export function BulkUploadButton({ onClick }) {
  return (
    <Button type="button" variant="secondary" onClick={onClick}>
      <Upload className="h-4 w-4" aria-hidden />
      Bulk upload
    </Button>
  );
}
