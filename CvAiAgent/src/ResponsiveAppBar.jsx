import * as React from 'react';
import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';
import Typography from '@mui/material/Typography';
import Container from '@mui/material/Container';
import AdbIcon from '@mui/icons-material/Adb';
import Box from '@mui/material/Box';
import AiTypingAnimation from './AiTypingAnimation';

function ResponsiveAppBar() {
  return (
    <AppBar position="static" sx={{ bgcolor: '#282C35'}}>
      <Container maxWidth="xl">
        <Toolbar disableGutters sx={{ justifyContent: 'center', position: 'relative' }}>
          <Box sx={{ position: 'absolute', left: 0 }}>
            <AiTypingAnimation />
          </Box>
          <Box display="flex" alignItems="center">
            <AdbIcon sx={{ mr: 1 }} />
            <Typography
              variant="h6"
              noWrap
              component="a"
              href="#"
              sx={{
                fontFamily: 'monospace',
                fontWeight: 700,
                letterSpacing: '.3rem',
                color: 'inherit',
                textDecoration: 'none',
              }}
            >
              CV AI AGENT
            </Typography>
          </Box>
        </Toolbar>
      </Container>
    </AppBar>
  );
}

export default ResponsiveAppBar;
