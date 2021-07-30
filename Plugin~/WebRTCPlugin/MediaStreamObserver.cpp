#include "pch.h"
#include "MediaStreamObserver.h"

namespace unity
{
namespace webrtc
{

    MediaStreamObserver::MediaStreamObserver(webrtc::MediaStreamInterface* stream) : 
        m_cachedAudioTrackList(stream->GetAudioTracks()),
        m_cachedVideoTrackList(stream->GetVideoTracks()),
        webrtc::MediaStreamObserver(stream)
    {
        this->SignalVideoTrackAdded.connect(this, &MediaStreamObserver::OnVideoTrackAdded);
        this->SignalAudioTrackAdded.connect(this, &MediaStreamObserver::OnAudioTrackAdded);
        this->SignalVideoTrackRemoved.connect(this, &MediaStreamObserver::OnVideoTrackRemoved);
        this->SignalAudioTrackRemoved.connect(this, &MediaStreamObserver::OnAudioTrackRemoved);
    }

    void MediaStreamObserver::OnVideoTrackAdded(webrtc::VideoTrackInterface* track, webrtc::MediaStreamInterface* stream)
    {
        DebugLog("OnVideoTrackAdded trackId:%s, streamId:%s", track->id().c_str(), stream->id().c_str());
        m_cachedVideoTrackList.push_back(track);
        for (auto callback : m_listOnAddTrack)
        {
            callback(stream, track);
        }
    }

    void MediaStreamObserver::OnAudioTrackAdded(webrtc::AudioTrackInterface* track, webrtc::MediaStreamInterface* stream)
    {
        DebugLog("OnAudioTrackAdded trackId:%s, streamId:%s", track->id().c_str(), stream->id().c_str());
        m_cachedAudioTrackList.push_back(track);
        for (auto callback : m_listOnAddTrack)
        {
            callback(stream, track);
        }
    }

    void MediaStreamObserver::OnVideoTrackRemoved(webrtc::VideoTrackInterface* track, webrtc::MediaStreamInterface* stream)
    {
        DebugLog("OnVideoTrackRemoved trackId:%s, streamId:%s", track->id().c_str(), stream->id().c_str());
        for (auto callback : m_listOnRemoveTrack)
        {
            callback(stream, track);
        }
    }

    void MediaStreamObserver::OnAudioTrackRemoved(webrtc::AudioTrackInterface* track, webrtc::MediaStreamInterface* stream)
    {
        DebugLog("OnAudioTrackRemoved trackId:%s, streamId:%s", track->id().c_str(), stream->id().c_str());
        for (auto callback : m_listOnRemoveTrack)
        {
            callback(stream, track);
        }
    }

    void MediaStreamObserver::RegisterOnAddTrack(DelegateMediaStreamOnAddTrack callback)
    {
        m_listOnAddTrack.push_back(callback);
    }

    void MediaStreamObserver::RegisterOnRemoveTrack(DelegateMediaStreamOnRemoveTrack callback)
    {
        m_listOnRemoveTrack.push_back(callback);
    }

    void MediaStreamObserver::RemoveCachedTrack(const char* trackId)
    {
        const auto result1 = std::find_if(m_cachedVideoTrackList.begin(), m_cachedVideoTrackList.end(),[trackId](rtc::scoped_refptr<webrtc::VideoTrackInterface> x){ return x.get()->id() == trackId; });
        if (result1 != m_cachedVideoTrackList.end())
        {
            m_cachedVideoTrackList.erase(result1);
        }

        const auto result2 = std::find_if(m_cachedAudioTrackList.begin(), m_cachedAudioTrackList.end(),[trackId](rtc::scoped_refptr<webrtc::AudioTrackInterface> x){ return x.get()->id() == trackId; });
        if (result2 != m_cachedAudioTrackList.end())
        {
            m_cachedAudioTrackList.erase(result2);
        }
    }

} // end namespace webrtc
} // end namespace unity
