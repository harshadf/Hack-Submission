import React, { useEffect, useState } from 'react';
import { Stack, TextareaAutosize, Button } from '@mui/material';
import { usePromptStore } from './store/usePromptStore';
function JobDescription() {
  const firstText = usePromptStore((state) => state.firstText);
  const setFirstText = usePromptStore((state) => state.setFirstText);
  const [isButtonDisable, setIsButtonDisable] = useState(true);
  
  const clearJobDescription = () => {
    setFirstText('');
  }
  useEffect(() => {
    console.log(firstText);
    if (firstText == '') {
      setIsButtonDisable(true);
      return;
    }
    setIsButtonDisable(false);
  }, [firstText]);
  return (
    <Stack
      spacing={2}
      sx={{
        width: '350px',
        height: '400px',
        alignItems: 'center',
        p: 2 
      }}
    >
      <textarea
        placeholder="Enter the job description here"
        value={firstText}
        onChange={(e) => setFirstText(e.target.value)}
        style={{
          width: '350px',
          height: '350px', 
          padding: '8px',
          borderRadius: '4px',
          border: '1px solid #ccc',
          resize: 'none',
          fontSize: '16px',
          fontFamily: 'Arial, sans-serif',
          overflow: 'auto',
        }}
      />

      <Button
        variant="contained"
        onClick={clearJobDescription}
        disabled={isButtonDisable}
        style={{
          width: '350px'
        }}
      >
      Clear
      </Button>
    </Stack>
  );
}

export default JobDescription;
