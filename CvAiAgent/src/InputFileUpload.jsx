import React, { useEffect, useState } from 'react';
import { Button, Typography, Stack } from '@mui/material';
import UploadFileIcon from '@mui/icons-material/UploadFile';
import axios from '../src/axios';

function InputFileUpload() {
  const [fileNames, setFileNames] = useState([]);
  const [uploading, setUploading] = useState(false);
  const [fileResponse, setFileResponse] = useState(null);
  const [succussMessage, setSuccessMessage] = useState("");

  useEffect(() => {
    setSuccessMessage('Files are uploading...');
  }, [fileNames]);
  useEffect(() => {
    setSuccessMessage('Files uploaded successfully!');
  }, [fileResponse]);

  const handleFileChange = async (event) => {
    const files = event.target.files;
    if (!files || files.length === 0) return;

    const formData = new FormData();

    Array.from(files).forEach((file) => {
      formData.append('files', file);
    });

    setFileNames(Array.from(files).map((f) => f.name));

    try {
      
      setUploading(true);
      const response = await axios.post('UploadCV', formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });
      setFileResponse(response.data);
      console.log('Files uploaded successfully', response.data);
      alert('Files uploaded successfully!');
    } catch (error) {
      console.error('Error uploading files', error);
      alert('Failed to upload files.');
    } finally {
      setUploading(false);
    }
  };

  return (
    <Stack
      spacing={2}
      sx={{
        width: '300px',
        alignItems: 'center',
      }}
    >
      <Typography variant="h6">Upload Your CV</Typography>

      <Button
        variant="outlined"
        component="label"
        startIcon={<UploadFileIcon />}
        sx={{
          width: '100%',
          height: '56px',
          textTransform: 'none',
        }}
        disabled={uploading}
      >
        {fileNames.length > 0 ? succussMessage : 'Upload file(s)'}
        <input
          type="file"
          hidden
          multiple
          accept="application/pdf"
          onChange={handleFileChange}
        />
      </Button>
    </Stack>
  );
}

export default InputFileUpload;