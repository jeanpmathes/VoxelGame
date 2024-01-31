//  <copyright file="StepTimer.hpp" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      MIT License
//  </copyright>
//  <author>Microsoft</author>

#pragma once

#include <cmath>
#include <cstdint>
#include <exception>

/**
 * \brief  Helper class for animation and simulation timing.
 */
class StepTimer
{
public:
    StepTimer() noexcept(false) :
        m_elapsedTicks(0),
        m_totalTicks(0),
        m_leftOverTicks(0),
        m_frameCount(0),
        m_framesPerSecond(0),
        m_framesThisSecond(0),
        m_qpcSecondCounter(0),
        m_isFixedTimeStep(false),
        m_targetElapsedTicks(TICKS_PER_SECOND / 60)
    {
        if (!QueryPerformanceFrequency(&m_qpcFrequency))
        {
            throw NativeException("Failed to query performance frequency.");
        }

        if (!QueryPerformanceCounter(&m_qpcLastTime))
        {
            throw NativeException("Failed to query performance counter.");
        }

        m_qpcMaxDelta = static_cast<uint64_t>(m_qpcFrequency.QuadPart / 10);
    }

    [[nodiscard]] uint64_t GetElapsedTicks() const noexcept { return m_elapsedTicks; }
    [[nodiscard]] double GetElapsedSeconds() const noexcept { return TicksToSeconds(m_elapsedTicks); }

    [[nodiscard]] uint64_t GetTotalTicks() const noexcept { return m_totalTicks; }
    [[nodiscard]] double GetTotalSeconds() const noexcept { return TicksToSeconds(m_totalTicks); }

    [[nodiscard]] uint32_t GetFrameCount() const noexcept { return m_frameCount; }

    [[nodiscard]] uint32_t GetFramesPerSecond() const noexcept { return m_framesPerSecond; }

    void SetFixedTimeStep(const bool isFixedTimestep) noexcept { m_isFixedTimeStep = isFixedTimestep; }

    void SetTargetElapsedTicks(const uint64_t targetElapsed) noexcept { m_targetElapsedTicks = targetElapsed; }

    void SetTargetElapsedSeconds(const double targetElapsed) noexcept
    {
        m_targetElapsedTicks = SecondsToTicks(targetElapsed);
    }

    static constexpr uint64_t TICKS_PER_SECOND = 10000000;

    static constexpr double TicksToSeconds(const uint64_t ticks) noexcept
    {
        return static_cast<double>(ticks) / TICKS_PER_SECOND;
    }

    static constexpr uint64_t SecondsToTicks(const double seconds) noexcept
    {
        return static_cast<uint64_t>(seconds * TICKS_PER_SECOND);
    }

    void ResetElapsedTime()
    {
        if (!QueryPerformanceCounter(&m_qpcLastTime))
        {
            throw NativeException("Failed to query performance counter.");
        }

        m_leftOverTicks = 0;
        m_framesPerSecond = 0;
        m_framesThisSecond = 0;
        m_qpcSecondCounter = 0;
    }

    template <typename TUpdate>
    void Tick(const TUpdate& update)
    {
        LARGE_INTEGER currentTime;

        if (!QueryPerformanceCounter(&currentTime))
        {
            throw NativeException("Failed to query performance counter.");
        }

        uint64_t timeDelta = static_cast<uint64_t>(currentTime.QuadPart - m_qpcLastTime.QuadPart);

        m_qpcLastTime = currentTime;
        m_qpcSecondCounter += timeDelta;

        if (timeDelta > m_qpcMaxDelta)
        {
            timeDelta = m_qpcMaxDelta;
        }

        timeDelta *= TICKS_PER_SECOND;
        timeDelta /= static_cast<uint64_t>(m_qpcFrequency.QuadPart);

        const uint32_t lastFrameCount = m_frameCount;

        if (m_isFixedTimeStep)
        {
            if (static_cast<uint64_t>(std::abs(static_cast<int64_t>(timeDelta - m_targetElapsedTicks))) <
                TICKS_PER_SECOND
                / 4000)
            {
                timeDelta = m_targetElapsedTicks;
            }

            m_leftOverTicks += timeDelta;

            while (m_leftOverTicks >= m_targetElapsedTicks)
            {
                m_elapsedTicks = m_targetElapsedTicks;
                m_totalTicks += m_targetElapsedTicks;
                m_leftOverTicks -= m_targetElapsedTicks;
                m_frameCount++;

                update();
            }
        }
        else
        {
            m_elapsedTicks = timeDelta;
            m_totalTicks += timeDelta;
            m_leftOverTicks = 0;
            m_frameCount++;

            update();
        }

        if (m_frameCount != lastFrameCount)
        {
            m_framesThisSecond++;
        }

        if (m_qpcSecondCounter >= static_cast<uint64_t>(m_qpcFrequency.QuadPart))
        {
            m_framesPerSecond = m_framesThisSecond;
            m_framesThisSecond = 0;
            m_qpcSecondCounter %= static_cast<uint64_t>(m_qpcFrequency.QuadPart);
        }
    }

private:
    LARGE_INTEGER m_qpcFrequency{};
    LARGE_INTEGER m_qpcLastTime{};
    uint64_t m_qpcMaxDelta;

    uint64_t m_elapsedTicks;
    uint64_t m_totalTicks;
    uint64_t m_leftOverTicks;

    uint32_t m_frameCount;
    uint32_t m_framesPerSecond;
    uint32_t m_framesThisSecond;
    uint64_t m_qpcSecondCounter;

    bool m_isFixedTimeStep;
    uint64_t m_targetElapsedTicks;
};
