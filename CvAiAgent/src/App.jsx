import React from 'react';
import { Box, Paper, Typography, Divider } from '@mui/material';
import ChatBox from './ChatBox';
import InputFileUpload from './InputFileUpload';
import JobDescription from './JobDescription';
import AiTypingAnimation from './AiTypingAnimation';
import ResponsiveAppBar from './ResponsiveAppBar';

function App() {
  return (
    <Box
      sx={{
        height: '100vh',
        width: '100%',
        display: 'flex',
        flexDirection: 'column',
        bgcolor: '#282C35',
      }}
    >
      {/* Top Navigation Bar */}
      <ResponsiveAppBar />

      {/* Main Content */}
      <Box
        sx={{
          display: 'flex',
          flex: 1,
          overflow: 'hidden',
          p: 1,
          gap: 3,
        }}
      >
        {/* Left Panel */}
        <Paper
          elevation={3}
          sx={{
            width: '100%',
            maxWidth: 420,
            overflowY: 'auto',
            overflowX: 'hidden',
            display: 'flex',
            flexDirection: 'column',
            p: 3,
            gap: 3,
            borderRadius: 3,
            bgcolor: '',
            boxSizing: 'border-box',
        
          }}
        >
          {/* AI Animation */}
          {/* <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <AiTypingAnimation />
            <Typography variant="subtitle2" color="text.secondary">
              AI is standing by...
            </Typography>
          </Box> */}

          <Divider />

          <Box>
            <Typography variant="subtitle1" gutterBottom>
              Upload Resume
            </Typography>
            <InputFileUpload />
          </Box>

          <Box>
            <Typography variant="subtitle1"f gutterBottom>
              Job Description
            </Typography>
            <JobDescription />
          </Box>
        </Paper>

        <Box
          sx={{
            flex: 1,
            display: 'flex',
            flexDirection: 'column'
          }}
        >
          <Paper
            elevation={3}
            sx={{
              flex: 1,
              p: 3,
              borderRadius: 3,
              display: 'flex',
              flexDirection: 'column',
              bgcolor: '#ffffff',
            }}
          >
            <ChatBox />
          </Paper>
        </Box>
      </Box>
    </Box>
  );
}

export default App;
