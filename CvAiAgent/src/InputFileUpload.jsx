import React, { useState } from 'react';
import { Button, Typography, Stack } from '@mui/material';
import UploadFileIcon from '@mui/icons-material/UploadFile';
import axios from 'axios';

function InputFileUpload() {
  const [fileName, setFileName] = useState('');
  const [uploading, setUploading] = useState(false);

  const handleFileChange = async (event) => {
    const file = event.target.files[0];
    if (!file) return;

    setFileName(file.name);

    const formData = new FormData();
    formData.append('file', file);

    try {
      setUploading(true);

      const response = await axios.post('https://localhost:7000/UploadCV', formData, {
        headers: {
          'Content-Type': 'multipart/form-data'
        }
      });

      console.log('File uploaded successfully', response.data);
      alert('File uploaded successfully!');
    } catch (error) {
      console.error('Error uploading file', error);
      alert('Failed to upload file.');
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
        {fileName ? fileName : 'Upload a file'}
        <input
          type="file"
          hidden
          onChange={handleFileChange}
        />
      </Button>
    </Stack>
  );
}

export default InputFileUpload;
