import React, { useEffect } from 'react';
import { Stack, Typography, TextareaAutosize } from '@mui/material';
import { usePromptStore } from './store/usePromptStore';
import axios from '../src/axios';

function JobDescription() {
  const firstText = usePromptStore((state) => state.firstText);
  const setFirstText = usePromptStore((state) => state.setFirstText);

  return (
    <Stack
      spacing={2}
      sx={{
        width: '300px',
        height: '400px',
        alignItems: 'center',
        p: 2 
      }}
    >
      {/* Job Description Topic */}
      <Typography variant="h6" gutterBottom>
        Job Description
      </Typography>

      {/* Job Description Input (Text Area) */}
      <TextareaAutosize
        minRows={4}
        maxRows={20}
        placeholder="Enter the job description here"
        value={firstText}
        onChange={(e) => setFirstText(e.target.value)}
        style={{
          width: '300px',
          height: '130px',
          padding: '8px',
          borderRadius: '4px',
          border: '1px solid #ccc',
          resize: 'none',
          fontSize: '16px',
          fontFamily: 'Arial, sans-serif',
        }}
      />
    </Stack>
  );
}

export default JobDescription;
